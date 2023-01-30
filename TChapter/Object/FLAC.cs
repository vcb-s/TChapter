// SPDX-License-Identifier: GPL-3.0-or-later
// SPDX-FileCopyrightText: Copyright 2017-2023 TautCony (i@tautcony.xyz)

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using Serilog;
using TChapter.Util;

namespace TChapter.Object
{
    public class FLACInfo
    {
        public long RawLength { get; set; }
        public long TrueLength { get; set; }
        public double CompressRate => TrueLength / (double)RawLength;
        public bool HasCover { get; set; }
        public string Encoder { get; set; }
        public Dictionary<string, string> VorbisComment { get; }

        public FLACInfo()
        {
            VorbisComment = new Dictionary<string, string>();
        }
    }

    //https://xiph.org/flac/format.html
    public sealed class FLAC
    {
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        private enum BlockType
        {
            STREAMINFO = 0x00,
            PADDING,
            APPLICATION,
            SEEKTABLE,
            VORBIS_COMMENT,
            CUESHEET,
            PICTURE
        };

        public static FLACInfo GetMetadataFromFlac(Stream fs)
        {
            if (fs == null)
            {
                throw new ArgumentNullException(nameof(fs));
            }
            var info = new FLACInfo { TrueLength = fs.Length };
            var header = Encoding.ASCII.GetString(fs.ReadBytes(4), 0, 4);
            if (header != "fLaC")
                throw new InvalidDataException($"Except an flac but get an {header}");
            //METADATA_BLOCK_HEADER
            //1-bit Last-metadata-block flag
            //7-bit BLOCK_TYPE
            //24-bit Length
            while (fs.Position < fs.Length)
            {
                var blockHeader = fs.BEInt32();
                var lastMetadataBlock = blockHeader >> 31 == 0x1;
                var blockType = (BlockType)((blockHeader >> 24) & 0x7f);
                var length = blockHeader & 0xffffff;
                info.TrueLength -= length;
                Log.Information("|+{BlockType} with Length: {Length}", blockType, length);
                switch (blockType)
                {
                    case BlockType.STREAMINFO:
                        Debug.Assert(length == 34, "Stream info block length must be 34");
                        ParseStreamInfo(fs, ref info);
                        break;
                    case BlockType.VORBIS_COMMENT:
                        ParseVorbisComment(fs, ref info);
                        break;
                    case BlockType.PICTURE:
                        ParsePicture(fs, ref info);
                        break;
                    case BlockType.PADDING:
                    case BlockType.APPLICATION:
                    case BlockType.SEEKTABLE:
                    case BlockType.CUESHEET:
                        fs.Seek(length, SeekOrigin.Current);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException($"Invalid BLOCK_TYPE: 0x{blockType:X2}");
                }
                if (lastMetadataBlock) break;
            }
            return info;
        }

        private static void ParseStreamInfo(Stream fs, ref FLACInfo info)
        {
            var minBlockSize = fs.BEInt16();
            var maxBlockSize = fs.BEInt16();
            var minFrameSize = fs.BEInt24();
            var maxFrameSize = fs.BEInt24();
            var buffer = fs.ReadBytes(8);
            var br = new BitReader(buffer);
            var sampleRate = br.GetBits(20);
            var channelCount = br.GetBits(3) + 1;
            var bitPerSample = br.GetBits(5) + 1;
            var totalSample = br.GetBits(36);
            var md5 = fs.ReadBytes(16);
            info.RawLength = channelCount * bitPerSample / 8 * totalSample;
            Log.Information(" | minimum block size: {MinBlockSize}, maximum block size: {MaxBlockSize}", minBlockSize, maxBlockSize);
            Log.Information(" | minimum frame size: {MinFrameSize}, maximum frame size: {MaxFrameSize}", minFrameSize, maxFrameSize);
            Log.Information(" | Sample rate: {SampleRate}Hz, bits per sample: {BitPerSample}-bit", sampleRate, bitPerSample);
            Log.Information(" | Channel count: {ChannelCount}", channelCount);
            var md5String = md5.Aggregate("", (current, item) => current + $"{item:X2}");
            Log.Information(" | MD5: {Md5}", md5String);
        }

        private static void ParseVorbisComment(Stream fs, ref FLACInfo info)
        {
            //only here in flac use little-endian
            var vendorLength = (int)fs.LEInt32();
            var vendorRawStringData = fs.ReadBytes(vendorLength);
            var vendor = Encoding.UTF8.GetString(vendorRawStringData, 0, vendorLength);
            info.Encoder = vendor;
            Log.Information(" | Vendor: {Vendor}", vendor);
            var userCommentListLength = fs.LEInt32();
            for (var i = 0; i < userCommentListLength; ++i)
            {
                var commentLength = (int)fs.LEInt32();
                var commentRawStringData = fs.ReadBytes(commentLength);
                var comment = Encoding.UTF8.GetString(commentRawStringData, 0, commentLength);
                var splitterIndex = comment.IndexOf('=');
                var key = comment.Substring(0, splitterIndex);
                var value = comment.Substring(splitterIndex + 1, comment.Length - 1 - splitterIndex);
                info.VorbisComment[key] = value;
                var summary = value.Length > 25 ? value.Substring(0, 25) + "..." : value;
                Log.Information(" | [{Key}] = '{Summary}'", key, summary.Replace('\n', ' '));
            }
        }

        private static readonly string[] PictureTypeName =
        {
            "Other", "32x32 pixels 'file icon'", "Other file icon",
            "Cover (front)", "Cover (back)", "Leaflet page",
            "Media", "Lead artist/lead performer/soloist", "Artist/performer",
            "Conductor", "Band/Orchestra", "Composer",
            "Lyricist/text writer", "Recording Location", "During recording",
            "During performance", "Movie/video screen capture", "A bright coloured fish",
            "Illustration", "Band/artist logotype", "Publisher/Studio logotype",
            "Reserved"
        };

        private static void ParsePicture(Stream fs, ref FLACInfo info)
        {
            var pictureType = fs.BEInt32();
            var mimeStringLength = (int)fs.BEInt32();
            var mimeType = Encoding.ASCII.GetString(fs.ReadBytes(mimeStringLength), 0, mimeStringLength);
            var descriptionLength = (int)fs.BEInt32();
            var description = Encoding.UTF8.GetString(fs.ReadBytes(descriptionLength), 0, descriptionLength);
            var pictureWidth = fs.BEInt32();
            var pictureHeight = fs.BEInt32();
            var colorDepth = fs.BEInt32();
            var indexedColorCount = fs.BEInt32();
            var pictureDataLength = fs.BEInt32();
            fs.Seek(pictureDataLength, SeekOrigin.Current);
            info.TrueLength -= pictureDataLength;
            info.HasCover = true;
            if (pictureType > 20) pictureType = 21;
            Log.Information(" | picture type: {PictureTypeName}", PictureTypeName[pictureType]);
            Log.Information(" | picture format type: {MimeType}", mimeType);
            if (descriptionLength > 0)
                Log.Information(" | description: {Description}", description);
            Log.Information(" | attribute: {PictureWidth}px*{PictureHeight}px@{ColorDepth}-bit", pictureWidth, pictureHeight, colorDepth);
            if (indexedColorCount != 0)
                Log.Information(" | indexed-color color: {IndexedColorCount}", indexedColorCount);
        }
    }
}
