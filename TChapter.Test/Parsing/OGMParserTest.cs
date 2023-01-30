// SPDX-License-Identifier: GPL-3.0-or-later
// SPDX-FileCopyrightText: Copyright 2017-2023 TautCony (i@tautcony.xyz)

using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TChapter.Chapters;
using TChapter.Parsing;

namespace TChapter.Test.Parsing
{
    [TestClass]
    public class OGMParserTest
    {
        [TestMethod]
        public void TestParseOGM()
        {
            IChapterParser parser = new OGMParser();
            var data = parser.Parse(Path.Combine(Configuration.TestCaseBasePath, "OGM", "00001.txt"));
            Console.WriteLine(data);
            foreach (var chapter in (data as SingleChapterData).Chapters)
            {
                Console.WriteLine(chapter);
            }
        }
    }
}
