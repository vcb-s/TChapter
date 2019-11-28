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
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TChapter.Chapters;
using TChapter.Parsing;
using TChapter.Util;

namespace TChapter.Test.Parsing
{
    [TestClass]
    public class IFOParserTest
    {
        [TestMethod]
        public void TestParseIFO()
        {
            var expectResult = new[]
            {
                new { Name = "Chapter 01", Time = "00:00:00.000" },
                new { Name = "Chapter 02", Time = "00:17:43.562" },
                new { Name = "Chapter 03", Time = "00:37:17.001" },
                new { Name = "Chapter 04", Time = "00:56:27.551" },
                new { Name = "Chapter 05", Time = "01:12:41.057" },
                new { Name = "Chapter 06", Time = "01:32:31.813" },
                new { Name = "Chapter 07", Time = "01:49:12.679" }
            };

            IChapterParser parser = new IFOParser();
            var data = parser.Parse(Path.Combine(Configuration.TestCaseBasePath, "IFO", "VTS_05_0.IFO"));
            Console.WriteLine(data);

            Assert.IsTrue(data.Count == 1);
            Assert.IsTrue(data[0].Chapters.Count == 7);
            var index = 0;
            foreach (var chapter in data[0].Chapters)
            {
                Console.WriteLine(chapter);
                expectResult[index].Name.Should().Be(chapter.Name);
                expectResult[index].Time.Should().Be(chapter.Time.Time2String());
                ++index;
            }
        }
    }
}
