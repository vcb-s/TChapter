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
using System.ComponentModel;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TChapter.Chapters;
using TChapter.Parsing;

namespace TChapter.Test.Parsing
{
    [TestClass]
    public class BDMVParserTest
    {
        [TestMethod]
        public void TestParseBDMV()
        {
            if (Configuration.CurrentPlatform != Platform.Windows)
            {
                Console.WriteLine("eac3to is only supported on Windows.");
                return;
            }
            if (!File.Exists(@"C:\Tools\_media\eac3to\eac3to.exe"))
            {
                Console.WriteLine("eac3to not found, skip this test case.");
                return;
            }

            IChapterParser parser = new BDMVParser(@"C:\Tools\_media\eac3to\eac3to.exe");
            try
            {
                var data = parser.Parse(Path.Combine(Configuration.TestCaseBasePath, "BDMV", "DISC1"));
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
            catch (Win32Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}
