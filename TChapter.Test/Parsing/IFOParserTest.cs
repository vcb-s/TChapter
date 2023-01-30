// SPDX-License-Identifier: GPL-3.0-or-later
// SPDX-FileCopyrightText: Copyright 2017-2023 TautCony (i@tautcony.xyz)

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
