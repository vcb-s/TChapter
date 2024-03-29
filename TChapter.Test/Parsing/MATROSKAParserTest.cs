﻿// SPDX-License-Identifier: GPL-3.0-or-later
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
        public static string GetFullPath(string fileName)
        {
            if (File.Exists(fileName))
                return Path.GetFullPath(fileName);

            var values = Environment.GetEnvironmentVariable("PATH") ?? "";
            foreach (var path in values.Split(Path.PathSeparator))
            {
                var fullPath = Path.Combine(path, fileName);
                if (File.Exists(fullPath))
                    return fullPath;
            }
            return null;
        }
        
        [TestMethod]
        public void TestParseMKV()
        {
            var mkvextractPath = Configuration.CurrentPlatform == Platform.Windows
                ? @"C:\Program Files\MKVToolNix\mkvextract.exe"
                : GetFullPath("mkvextract");
            
            if (!File.Exists(mkvextractPath))
            {
                Console.WriteLine("mkvextract not found, skip this test case.");
                return;
            }

            IChapterParser parser = new MATROSKAParser(mkvextractPath);
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
