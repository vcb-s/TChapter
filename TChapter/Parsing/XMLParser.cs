// SPDX-License-Identifier: GPL-3.0-or-later
// SPDX-FileCopyrightText: Copyright 2017-2023 TautCony (i@tautcony.xyz)

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
