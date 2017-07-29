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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using TChapter.Chapters;

namespace TChapter.Parsing
{
    public class XPLParser : IChapterParser
    {
        public IChapterData Parse(string path)
        {
            var chapters = new MultiChapterData(ChapterTypeEnum.XPL);
            chapters.AddRange(GetStreams(path));
            return chapters;
        }

        public IChapterData Parse(Stream stream)
        {
            throw new InvalidDataException();
        }

        public static IEnumerable<ChapterInfo> GetStreams(string location)
        {
            var doc = XDocument.Load(location);
            XNamespace ns = "http://www.dvdforum.org/2005/HDDVDVideo/Playlist";
            foreach (var ts in doc.Element(ns + "Playlist").Elements(ns + "TitleSet"))
            {
                var timeBase = GetFps((string)ts.Attribute("timeBase")) ?? 60; //required
                var tickBase = GetFps((string)ts.Attribute("tickBase")) ?? 24; //optional
                foreach (var title in ts.Elements(ns + "Title").Where(t => t.Element(ns + "ChapterList") != null))
                {
                    var pgc = new ChapterInfo
                    {
                        SourceName = title.Element(ns + "PrimaryAudioVideoClip")?.Attribute("src")?.Value ?? "",
                        FramesPerSecond = 24D,
                        Chapters = new List<Chapter>()
                    };
                    var tickBaseDivisor = (int?)title.Attribute("tickBaseDivisor") ?? 1; //optional
                    pgc.Duration = GetTimeSpan((string)title.Attribute("titleDuration"), timeBase, tickBase, tickBaseDivisor);
                    var titleName = Path.GetFileNameWithoutExtension(location);
                    if (title.Attribute("id") != null) titleName = title.Attribute("id")?.Value ?? ""; //optional
                    if (title.Attribute("displayName") != null) titleName = title.Attribute("displayName")?.Value ?? ""; //optional
                    pgc.Title = titleName;
                    foreach (var chapter in title.Element(ns + "ChapterList").Elements(ns + "Chapter"))
                    {
                        var chapterName = string.Empty;
                        if (chapter.Attribute("id") != null) chapterName = chapter.Attribute("id")?.Value ?? ""; //optional
                        if (chapter.Attribute("displayName") != null) chapterName = chapter.Attribute("displayName")?.Value ?? ""; //optional
                        pgc.Chapters.Add(new Chapter
                        {
                            Name = chapterName,
                            Time = GetTimeSpan((string)chapter.Attribute("titleTimeBegin"), timeBase, tickBase, tickBaseDivisor) //required
                        });
                    }
                    yield return pgc;
                }
            }
        }

        /// <summary>
        /// Eg: Convert string "\d+fps" to a double value
        /// </summary>
        /// <param name="fps"></param>
        /// <returns></returns>
        private static double? GetFps(string fps)
        {
            if (string.IsNullOrEmpty(fps)) return null;
            fps = fps.Replace("fps", string.Empty);
            return float.Parse(fps);
        }

        /// <summary>
        /// Constructs a TimeSpan from a string formated as "HH:MM:SS:TT"
        /// </summary>
        /// <param name="timeSpan"></param>
        /// <param name="timeBase"></param>
        /// <param name="tickBase"></param>
        /// <param name="tickBaseDivisor"></param>
        /// <returns></returns>
        private static TimeSpan GetTimeSpan(string timeSpan, double timeBase, double tickBase, int tickBaseDivisor)
        {
            var colonPosition = timeSpan.LastIndexOf(':');
            var ts = TimeSpan.Parse(timeSpan.Substring(0, colonPosition));
            ts = new TimeSpan((long)(ts.TotalSeconds / 60D * timeBase) * TimeSpan.TicksPerSecond);

            //convert ticks to ticks timebase
            var newTick = TimeSpan.TicksPerSecond / ((decimal)tickBase / tickBaseDivisor);
            var ticks = decimal.Parse(timeSpan.Substring(colonPosition + 1)) * newTick;
            return ts.Add(new TimeSpan((long)ticks));
        }
    }
}