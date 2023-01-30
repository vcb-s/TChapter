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
    public class MP4ParserTest
    {
        [TestMethod]
        public void TestParseMP4_nero()
        {
            if (Configuration.CurrentPlatform != Platform.Windows)
            {
                Console.WriteLine("mp4 file is only supported on Windows.");
                return;
            }

            if (Environment.Is64BitProcess)
            {
                Console.WriteLine("Current mp4v2.dll do not support x64.");
                return;
            }

            IChapterParser parser = new MP4Parser();
            var data = parser.Parse(Path.Combine(Configuration.TestCaseBasePath, "MP4", "nero.mp4"));
            Console.WriteLine(data);
            foreach (var chapter in (data as SingleChapterData).Chapters)
            {
                Console.WriteLine(chapter);
            }
        }
    }
}
