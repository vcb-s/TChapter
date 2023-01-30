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
