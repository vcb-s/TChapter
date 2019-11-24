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
            var data = parser.Parse(@"..\..\..\Assets\CUE\example-cue-sheet.cue");
            (data as SingleChapterData).Chapters.Select(i => i.Time).Should().BeEquivalentTo(expected);
        }

        [TestMethod]
        public void TestParseFLAC()
        {
            IChapterParser parser = new FLACParser();
            var data = parser.Parse(@"..\..\..\Assets\CUE\example-cue-sheet.flac");
            (data as SingleChapterData).Chapters.Select(i => i.Time).Should().BeEquivalentTo(_expected);
        }

        [TestMethod]
        public void TestParseTAK()
        {
            IChapterParser parser = new TAKParser();
            var data = parser.Parse(@"..\..\..\Assets\CUE\example-cue-sheet.tak");
            (data as SingleChapterData).Chapters.Select(i => i.Time).Should().BeEquivalentTo(_expected);
        }
    }
}
