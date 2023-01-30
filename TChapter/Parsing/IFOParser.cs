// SPDX-License-Identifier: GPL-3.0-or-later
// Copyright (C) 2009-2015 Kurtnoise (kurtnoise@free.fr)
// SPDX-FileCopyrightText: Copyright 2017-2023 TautCony


using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using TChapter.Chapters;
using TChapter.Util;

namespace TChapter.Parsing
{
    public class IFOParser : IChapterParser
    {
        public IChapterData Parse(string path)
        {
            var ret = new MultiChapterData(ChapterTypeEnum.IFO);
            ret.AddRange(GetStreams(path).Where(item => item != null));
            return ret;
        }

        public IChapterData Parse(Stream stream)
        {
            throw new InvalidDataException();
        }

        private static IEnumerable<ChapterInfo> GetStreams(string ifoFile)
        {
            var pgcCount = IFOParserUtil.GetPGCnb(ifoFile);
            for (var i = 1; i <= pgcCount; i++)
            {
                yield return GetChapterInfo(ifoFile, i);
            }
        }

        private static ChapterInfo GetChapterInfo(string location, int titleSetNum)
        {
            var titleRegex = new Regex(@"^VTS_(\d+)_0\.IFO", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            var result = titleRegex.Match(location);
            if (result.Success) titleSetNum = int.Parse(result.Groups[1].Value);

            var pgc = new ChapterInfo();
            var fileName = Path.GetFileNameWithoutExtension(location);
            Debug.Assert(fileName != null);
            if (fileName.Count(ch => ch == '_') == 2)
            {
                var barIndex = fileName.LastIndexOf('_');
                pgc.Title = pgc.SourceName = $"{fileName.Substring(0, barIndex)}_{titleSetNum}";
            }

            pgc.Chapters = GetChapters(location, titleSetNum, out var duration, out var isNTSC);
            pgc.Duration = duration;
            pgc.FramesPerSecond = isNTSC ? 30000M / 1001 : 25;

            if (pgc.Duration.TotalSeconds < 10)
                pgc = null;

            return pgc;
        }

        private static List<Chapter> GetChapters(string ifoFile, int programChain, out IfoTimeSpan duration, out bool isNTSC)
        {
            var chapters = new List<Chapter>();
            duration = IfoTimeSpan.Zero;
            isNTSC = false;

            using (var stream = new FileStream(ifoFile, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                var pcgItPosition = stream.GetPCGIP_Position();
                var programChainPrograms = -1;
                var programTime = TimeSpan.Zero;
                if (programChain >= 0)
                {
                    var chainOffset = stream.GetChainOffset(pcgItPosition, programChain);
                    //programTime = stream.ReadTimeSpan(pcgItPosition, chainOffset, out fpsLocal) ?? TimeSpan.Zero;
                    programChainPrograms = stream.GetNumberOfPrograms(pcgItPosition, chainOffset);
                }
                else
                {
                    var programChains = stream.GetProgramChains(pcgItPosition);
                    for (var curChain = 1; curChain <= programChains; curChain++)
                    {
                        var chainOffset = stream.GetChainOffset(pcgItPosition, curChain);
                        var time = stream.ReadTimeSpan(pcgItPosition, chainOffset, out _);
                        if (time == null) break;

                        if (time.Value <= programTime) continue;
                        programChain = curChain;
                        programChainPrograms = stream.GetNumberOfPrograms(pcgItPosition, chainOffset);
                        programTime = time.Value;
                    }
                }
                if (programChain < 0) return null;

                chapters.Add(new Chapter { Name = ChapterName.Get(1), Time = TimeSpan.Zero });

                var longestChainOffset = stream.GetChainOffset(pcgItPosition, programChain);
                int programMapOffset = IFOParserUtil.ToInt16(stream.GetFileBlock((pcgItPosition + longestChainOffset) + 230, 2));
                int cellTableOffset = IFOParserUtil.ToInt16(stream.GetFileBlock((pcgItPosition + longestChainOffset) + 0xE8, 2));
                for (var currentProgram = 0; currentProgram < programChainPrograms; ++currentProgram)
                {
                    int entryCell = stream.GetFileBlock(((pcgItPosition + longestChainOffset) + programMapOffset) + currentProgram, 1)[0];
                    var exitCell = entryCell;
                    if (currentProgram < (programChainPrograms - 1))
                        exitCell = stream.GetFileBlock(((pcgItPosition + longestChainOffset) + programMapOffset) + (currentProgram + 1), 1)[0] - 1;

                    var totalTime = IfoTimeSpan.Zero;
                    for (var currentCell = entryCell; currentCell <= exitCell; currentCell++)
                    {
                        var cellStart = cellTableOffset + ((currentCell - 1) * 0x18);
                        var bytes = stream.GetFileBlock((pcgItPosition + longestChainOffset) + cellStart, 4);
                        var cellType = bytes[0] >> 6;
                        if (cellType == 0x00 || cellType == 0x01)
                        {
                            bytes = stream.GetFileBlock(((pcgItPosition + longestChainOffset) + cellStart) + 4, 4);
                            var time = IFOParserUtil.ReadTimeSpan(bytes, out isNTSC) ?? IfoTimeSpan.Zero;
                            totalTime += time;
                        }
                    }

                    //add a constant amount of time for each chapter?
                    //totalTime += TimeSpan.FromMilliseconds(fps != 0 ? (double)1000 / fps / 8D : 0);

                    duration += totalTime;
                    if (currentProgram + 1 < programChainPrograms)
                        chapters.Add(new Chapter { Name = ChapterName.Get(currentProgram + 2), Time = duration });
                }
                return chapters;
            }
        }
    }
}