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
    public class VTTParserTest
    {
        [TestMethod]
        public void TestParseVTT()
        {
            IChapterParser parser = new VTTParser();
            var data = parser.Parse(Path.Combine(Configuration.TestCaseBasePath, "VTT", "00001.vtt"));
            Console.WriteLine(data);
            foreach (var chapter in (data as SingleChapterData).Chapters)
            {
                Console.WriteLine(chapter);
            }
        }
    }
}
