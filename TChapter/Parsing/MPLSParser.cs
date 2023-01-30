// SPDX-License-Identifier: GPL-3.0-or-later
// SPDX-FileCopyrightText: Copyright 2017-2023 TautCony (i@tautcony.xyz)

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Serilog;
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
                    Log.Warning("PlayItem without any marks, index: {Index}", index);
                    info.Chapters = new List<Chapter> { new Chapter { Time = PTS2Time(0), Number = 1, Name = "Chapter 1" } };
                    chapters.Add(info);
                    continue;
                }
                var offset = data.Marks.First(filter).MarkTimeStamp;
                if (playItem.TimeInfo.INTime < offset)
                {
                    Log.Information("{{PlayItems[{I}]: first time stamp => {Offset}, in time => {InTime}}}", i, offset, playItem.TimeInfo.INTime);
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