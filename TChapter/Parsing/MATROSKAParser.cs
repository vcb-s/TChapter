﻿// SPDX-License-Identifier: GPL-3.0-or-later
// SPDX-FileCopyrightText: Copyright 2017-2023 TautCony (i@tautcony.xyz)

using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using TChapter.Chapters;

namespace TChapter.Parsing
{
    public class MATROSKAParser : IChapterParser
    {
        private readonly string _mkvextractPath;

        public MATROSKAParser(string MKVExtractPath)
        {
            _mkvextractPath = MKVExtractPath;
            if (!File.Exists(_mkvextractPath))
            {
                throw new Exception("mkvextract Path not found");
            }
        }

        public IChapterData Parse(string path)
        {
            return new XMLParser().Parse(GetChapters(path));
        }

        public IChapterData Parse(Stream stream)
        {
            throw new InvalidDataException();
        }

        public Stream GetChapters(string path)
        {
            var arg = $"chapters \"{path}\"";
            var result = RunMkvextract(arg, _mkvextractPath);
            if (result.StartsWith("Error", StringComparison.Ordinal))
                throw new Exception(result);
            if (string.IsNullOrEmpty(result)) throw new Exception("No Chapter Found");
            return new MemoryStream(Encoding.UTF8.GetBytes(result));
        }

        private static string RunMkvextract(string arguments, string program)
        {
            var process = new Process
            {
                StartInfo =
                {
                    FileName = program,
                    Arguments = arguments,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    StandardOutputEncoding = Encoding.UTF8
                }
            };
            process.Start();
            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            process.Close();
            return output;
        }
    }
}