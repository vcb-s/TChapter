// ****************************************************************************
//
// Copyright (C) 2017 TautCony (TautCony@vcb-s.com)
//
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.If not, see<http://www.gnu.org/licenses/>.
//
// ****************************************************************************

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
