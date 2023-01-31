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
using TChapter.Chapters;
using TChapter.Object;
using TChapter.Util;

namespace TChapter.Parsing
{
    public class MPLSParser : IChapterParser
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
            return GetChapters(new MPLS(stream));
        }

        public static IChapterData GetChapters(MPLS data)
        {
            var chapters = new MultiChapterData(ChapterTypeEnum.MPLS);
            for (var i = 0; i < data.PlayItems.Length; ++i)
            {
                var playItem = data.PlayItems[i];
                var attr = playItem.STNTable.StreamEntries.First(item => item is PrimaryVideoStreamEntry);
                var info = new ChapterInfo
                {
                    SourceName = data.PlayItems[i].FullName,
                    Duration = PTS2Time(playItem.TimeInfo.DeltaTime),
                    FramesPerSecond = Config.FRAME_RATE[attr.StreamAttributes.FrameRate]
                };

                var index = i;
                Func<Mark, bool> filter = item => item.MarkType == 0x01 && item.RefToPlayItemID == index;
                if (!data.Marks.Any(filter))
                {
                    Logger.Log($"PlayItem without any marks, index: {index}");
                    info.Chapters = new List<Chapter> { new Chapter { Time = PTS2Time(0), Number = 1, Name = "Chapter 1" } };
                    chapters.Add(info);
                    continue;
                }
                var offset = data.Marks.First(filter).MarkTimeStamp;
                if (playItem.TimeInfo.INTime < offset)
                {
                    Logger.Log($"{{PlayItems[{i}]: first time stamp => {offset}, in time => {playItem.TimeInfo.INTime}}}");
                    offset = playItem.TimeInfo.INTime;
                }
                var name = new ChapterName();
                info.Chapters = data.Marks.Where(filter).Select(mark => new Chapter
                {
                    Time = PTS2Time(mark.MarkTimeStamp - offset),
                    Number = name.Index,
                    Name = name.Get()
                }).ToList();
                chapters.Add(info);
            }
            return chapters;
        }

        public static TimeSpan PTS2Time(uint pts)
        {
            var total = pts / 45000M;
            var secondPart = Math.Floor(total);
            var millisecondPart = Math.Round((total - secondPart) * 1000M, MidpointRounding.AwayFromZero);
            return new TimeSpan(0, 0, 0, (int)secondPart, (int)millisecondPart);
        }
    }
}