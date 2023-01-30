// SPDX-License-Identifier: GPL-3.0-or-later
// SPDX-FileCopyrightText: Copyright 2017-2023 TautCony (i@tautcony.xyz)

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TChapter.Chapters;
using TChapter.Parsing;

namespace TChapter.Test.Parsing
{
    [TestClass]
    public class CUEParserTest
    {
        private readonly List<TimeSpan> _expected = new[]
        {
            0, 2.76
        }.Select(sec => new TimeSpan((long) Math.Round(sec * TimeSpan.TicksPerSecond))).ToList();

        [TestMethod]
        public void TestParseCUE()
        {
            var expected = new[]
            {
                0, 169.76, 354.307, 594.773, 761, 915.787, 1148.12, 1368.533, 1558.627, 1690.92,
                1946, 2305.307, 2579.12, 2906.547, 3270.013, 3456.507, 3652.667, 3950.387, 4277.707
            }.Select(sec => new TimeSpan((long)Math.Round(sec * TimeSpan.TicksPerSecond)));
            IChapterParser parser = new CUEParser();
            var data = parser.Parse(Path.Combine(Configuration.TestCaseBasePath, "CUE", "example-cue-sheet.cue"));
            (data as SingleChapterData).Chapters.Select(i => i.Time).Should().BeEquivalentTo(expected);
        }

        [TestMethod]
        public void TestParseFLAC()
        {
            IChapterParser parser = new FLACParser();
            var data = parser.Parse(Path.Combine(Configuration.TestCaseBasePath, "CUE", "example-cue-sheet.flac"));
            (data as SingleChapterData).Chapters.Select(i => i.Time).Should().BeEquivalentTo(_expected);
        }

        [TestMethod]
        public void TestParseTAK()
        {
            IChapterParser parser = new TAKParser();
            var data = parser.Parse(Path.Combine(Configuration.TestCaseBasePath, @"CUE", "example-cue-sheet.tak"));
            (data as SingleChapterData).Chapters.Select(i => i.Time).Should().BeEquivalentTo(_expected);
        }
    }
}
