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
    public class XMLParserTest
    {
        [TestMethod]
        public void TestParseXML_1()
        {
            IChapterParser parser = new XMLParser();
            var data = parser.Parse(Path.Combine(Configuration.TestCaseBasePath, "XML", "ordered_chapter.xml"));
            Console.WriteLine(data);
            foreach (var chapter in data as MultiChapterData)
            {
                foreach (var item in chapter.Chapters)
                {
                    Console.WriteLine(item);
                }
                Console.WriteLine();
            }
        }

        [TestMethod]
        public void TestParseXML_2()
        {
            IChapterParser parser = new XMLParser();
            var data = parser.Parse(Path.Combine(Configuration.TestCaseBasePath, "XML", "sub_chapter.xml"));
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
