// SPDX-License-Identifier: GPL-3.0-or-later
// SPDX-FileCopyrightText: Copyright 2017-2023 TautCony (i@tautcony.xyz)

using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using TChapter.Chapters;

namespace TChapter.Parsing
{
    public class VTTParser : IChapterParser
    {
        public IChapterData Parse(string path)
        {
            using (var stream = File.OpenRead(path))
            {
                return Parse(stream);
            }
        }

        public IChapterData Parse(Stream stream)
        {
            using (var reader = new StreamReader(stream, true))
            {
                return new SingleChapterData(ChapterTypeEnum.VTT)
                {
                    Data = GetChapterInfo(reader.ReadToEnd())
                };
            }
        }

        public static ChapterInfo GetChapterInfo(string text)
        {
            var info = new ChapterInfo();
            text = text.Replace("\r", "");
            var nodes = Regex.Split(text, "\n\n");
            if (nodes.Length < 1 || nodes[0].IndexOf("WEBVTT", StringComparison.Ordinal) < 0)
            {
                throw new Exception("ERROR: Empty or invalid file type");
            }
            var index = 0;
            nodes.Skip(1).ToList().ForEach(node =>
            {
                var lines = node.Split('\n');
                lines = lines.SkipWhile(line => line.IndexOf("-->", StringComparison.Ordinal) < 0).ToArray();
                if (lines.Length < 2)
                {
                    throw new Exception($"+Parser Failed: Happened at [{node}]");
                }
                var times = Regex.Split(lines[0], "-->").Select(TimeSpan.Parse).ToArray();
                info.Chapters.Add(new Chapter(lines[1], times[0], ++index));
            });
            return info;
        }
    }
}