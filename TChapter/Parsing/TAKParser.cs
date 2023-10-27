// SPDX-License-Identifier: GPL-3.0-or-later
// SPDX-FileCopyrightText: Copyright 2017-2023 TautCony (i@tautcony.xyz)

using System;
using System.IO;
using System.Text;
using TChapter.Chapters;

namespace TChapter.Parsing
{
    public class TAKParser : IChapterParser
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
            var header = new byte[4];
            var len = stream.Read(header, 0, 4);
            if (len != 4 || Encoding.ASCII.GetString(header, 0, 4) != "tBaK")
                throw new InvalidDataException($"Except an tak but get an {Encoding.ASCII.GetString(header, 0, 4)}");
            stream.Seek(Math.Max(-20480, -stream.Length), SeekOrigin.End);

            return new CUEParser().Parse(GetCueSheet(stream));
        }

        private static Stream GetCueSheet(Stream stream)
        {
            var state = 0;
            var beginPos = stream.Position;
            while (stream.Position < stream.Length)
            {
                var peak = (char) stream.ReadByte();
                if (peak >= 'A' && peak <= 'Z')
                    peak = (char) (peak - 'A' + 'a');
                switch (peak)
                {
                    case 'c': state = 1; break;//C
                    case 'u': state = state == 1 ? 2 : 0; break;//Cu
                    case 'e':
                        switch (state)
                        {
                            case 2: state = 3; break;//Cue
                            case 5: state = 6; break;//Cueshe
                            case 6: state = 7; break;//Cueshee
                            default: state = 0; break;
                        }
                        break;

                    case 's': state = state == 3 ? 4 : 0; break;//Cues
                    case 'h': state = state == 4 ? 5 : 0; break;//Cuesh
                    case 't': state = state == 7 ? 8 : 0; break;//Cuesheet
                    default: state = 0; break;
                }
                if (state != 8) continue;
                beginPos = stream.Position + 1;
                break;
            }
            //var controlCount = type == "flac" ? 3 : type == "tak" ? 6 : 0;
            const int controlCount = 6;
            long endPos = 0;
            state = 0;
            //查找终止符 0D 0A ? 00 00 00 (连续 controlCount 个终止符以上) (flac为3, tak为6)
            stream.Seek(beginPos, SeekOrigin.Begin);
            while (stream.Position < stream.Length)
            {
                switch (stream.ReadByte())
                {
                    case 0: state++; break;
                    default: state = 0; break;
                }
                if (state != controlCount) continue;
                endPos = stream.Position - controlCount; //指向0D 0A后的第一个字符
                break;
            }
            if (beginPos == 0 || endPos <= 1) return Stream.Null;

            stream.Seek(endPos - 3, SeekOrigin.Begin);
            if (stream.ReadByte() == '\x0D' && stream.ReadByte() == '\x0A')
                --endPos;

            var ret = new MemoryStream();
            stream.Seek(beginPos, SeekOrigin.Begin);
            CopyStream(stream, ret, (int) (endPos - beginPos + 1));
            ret.Seek(0, SeekOrigin.Begin);

            return ret;
        }

        public static void CopyStream(Stream input, Stream output, int bytes)
        {
            var buffer = new byte[1<<15];
            int read;
            while (bytes > 0 && (read = input.Read(buffer, 0, Math.Min(buffer.Length, bytes))) > 0)
            {
                output.Write(buffer, 0, read);
                bytes -= read;
            }
        }
    }
}
