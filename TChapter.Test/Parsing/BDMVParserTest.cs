// SPDX-License-Identifier: GPL-3.0-or-later
// SPDX-FileCopyrightText: Copyright 2017-2023 TautCony (i@tautcony.xyz)

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
