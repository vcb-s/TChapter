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
    }
}
