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
    public class XPLParserTest
    {
        [TestMethod]
        public void TestParseXPL()
        {
            IChapterParser parser = new XPLParser();
            
            var data = parser.Parse(Path.Combine(Configuration.TestCaseBasePath, "XPL", "VPLST000.XPL"));
            Console.WriteLine(data);
            foreach (var chapter in data)
            {
                foreach (var item in chapter.Chapters)
                {
                    Console.WriteLine(item);
                }
                Console.WriteLine();
            }

        }
    }
}
