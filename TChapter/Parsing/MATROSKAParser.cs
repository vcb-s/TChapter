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
            string arg = $"chapters \"{path}\"";
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