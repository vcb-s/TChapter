// SPDX-License-Identifier: GPL-3.0-or-later
// SPDX-FileCopyrightText: Copyright 2017-2023 TautCony (i@tautcony.xyz)

using System;
using System.IO;
using System.Text;
using TChapter.Chapters;
using TChapter.Object;

namespace TChapter.Parsing
{
    public class FLACParser: IChapterParser
    {
        public IChapterData Parse(string path)
        {
            using (var stream = File.OpenRead(path))
            {
                return Parse(stream);
            }
        }

        public IChapterData Parse(Stream stream)
        {
            var info = FLAC.GetMetadataFromFlac(stream);
            if (info.VorbisComment.ContainsKey("cuesheet"))
                return new CUEParser().Parse(new MemoryStream(Encoding.UTF8.GetBytes(info.VorbisComment["cuesheet"])));
            throw new Exception("No cuesheet found in FLAC file");
        }
    }
}
