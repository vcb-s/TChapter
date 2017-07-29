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

using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using TChapter.Chapters;
using TChapter.Chapters.Serializable;
using TChapter.Util;

namespace TChapter.Parsing
{
    public class XMLParser : IChapterParser
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
            using (var reader = new StreamReader(stream))
            {
                var chapters = new MultiChapterData(ChapterTypeEnum.XML);
                var data = (Chapters.Serializable.Chapters)new XmlSerializer(typeof(Chapters.Serializable.Chapters)).Deserialize(reader);
                chapters.AddRange(ToChapterInfo(data));
                return chapters;
            }
        }

        public static IEnumerable<ChapterInfo> ToChapterInfo(Chapters.Serializable.Chapters chapters)
        {
            var index = 0;
            foreach (var entry in chapters.EditionEntry)
            {
                var info = new ChapterInfo();
                foreach (var atom in entry.ChapterAtom)
                {
                    info.Chapters.AddRange(ToChapter(atom, ++index));
                }
                yield return info;
            }
        }

        private static IEnumerable<Chapter> ToChapter(ChapterAtom atom, int index)
        {
            if (atom.ChapterTimeStart != null)
            {
                var startChapter = new Chapter
                {
                    Number = index,
                    Time = Config.TIME_FORMAT.Match(atom.ChapterTimeStart).Value.ToTimeSpan(),
                    Name = atom.ChapterDisplay.ChapterString ?? ""
                };
                yield return startChapter;
            }
            if (atom.SubChapterAtom != null)
                foreach (var chapterAtom in atom.SubChapterAtom)
                {
                    foreach (var chapter in ToChapter(chapterAtom, index))
                    {
                        yield return chapter;
                    }
                }

            if (atom.ChapterTimeEnd != null)
            {
                var endChapter = new Chapter
                {
                    Number = index,
                    Time = Config.TIME_FORMAT.Match(atom.ChapterTimeEnd).Value.ToTimeSpan(),
                    Name = atom.ChapterDisplay.ChapterString ?? ""
                };
                yield return endChapter;
            }
        }
    }
}