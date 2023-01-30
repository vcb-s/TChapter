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
    public class MPLSParserTest
    {
        [TestMethod]
        public void TestParseMPLS_HD()
        {
            IChapterParser parser = new MPLSParser();
            var data = parser.Parse(Path.Combine(Configuration.TestCaseBasePath, "MPLS", "00001-HD.mpls"));
            Console.WriteLine(data);
            foreach (var chapter in data)
            {
                foreach (var item in chapter.Chapters)
                {
                    Console.WriteLine(item + " " + item.FramesInfo);
                }
                Console.WriteLine();
            }
        }

        [TestMethod]
        public void TestParseMPLS_UHD()
        {
            IChapterParser parser = new MPLSParser();
            var data = parser.Parse(Path.Combine(Configuration.TestCaseBasePath, "MPLS", "00001-UHD.mpls"));
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
        
        [TestMethod]
        public void TestParseMPLS_Empty()
        {
            IChapterParser parser = new MPLSParser();
            var data = parser.Parse(Path.Combine(Configuration.TestCaseBasePath, "MPLS", "00001-Empty.mpls"));
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
