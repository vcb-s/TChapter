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
    public class MATROSKAParserTest
    {
        [TestMethod]
        public void TestParseMKV()
        {
            if (Configuration.CurrentPlatform != Platform.Windows)
            {
                Console.WriteLine("mkv file is only supported on Windows.");
                return;
            }

            if (!File.Exists(@"C:\Program Files\MKVToolNix\mkvextract.exe"))
            {
                Console.WriteLine("mkvextract not found, skip this test case.");
                return;
            }
            IChapterParser parser = new MATROSKAParser(@"C:\Program Files\MKVToolNix\mkvextract.exe");
            var data = parser.Parse(Path.Combine(Configuration.TestCaseBasePath, "MKV", "00001.mkv"));
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
