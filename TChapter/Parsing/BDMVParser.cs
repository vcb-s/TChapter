﻿// SPDX-License-Identifier: GPL-3.0-or-later
// SPDX-FileCopyrightText: Copyright 2017-2023 TautCony (i@tautcony.xyz)

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Serilog;
using TChapter.Chapters;
using TChapter.Util;

namespace TChapter.Parsing
{
    public class BDMVParser : IChapterParser
    {
        private readonly string _eac3toPath;

        public BDMVParser(string EAC3TOPath)
        {
            _eac3toPath = EAC3TOPath;
        }

        public IChapterData Parse(string path)
        {
            return GetChapterAsync(path).Result;
        }

        public async Task<IChapterData> ParseAsync(string path)
        {
            return await GetChapterAsync(path);
        }

        public IChapterData Parse(Stream stream)
        {
            throw new InvalidDataException();
        }

        private static readonly Regex RDiskInfo = new Regex(@"\d\) (?<mpls>\d+\.mpls)(?: \(angle \d\))?, .*(?<dur>\d+:\d+:\d+)", RegexOptions.Compiled);

        public async Task<IChapterData> GetChapterAsync(string location)
        {
            var list = new MultiChapterData(ChapterTypeEnum.BDMV);
            var path = Path.Combine(location, "BDMV", "PLAYLIST");
            if (!Directory.Exists(path))
            {
                throw new FileNotFoundException("Blu-Ray disc structure not found.");
            }

            var metaPath = Path.Combine(location, "BDMV", "META", "DL");
            if (Directory.Exists(metaPath))
            {
                var xmlFile = Directory.GetFiles(metaPath).FirstOrDefault(file => file.ToLower().EndsWith(".xml"));
                if (xmlFile != null)
                {
                    var xmlText = File.ReadAllText(xmlFile);
                    var title = Regex.Match(xmlText, "<di:name>(?<title>[^<]*)</di:name>");
                    if (title.Success)
                    {
                        var bdmvTitle = title.Groups["title"].Value;
                        Log.Information("Disc Title: {BdmvTitle}", bdmvTitle);
                    }
                }
            }

            var workingPath = Directory.GetParent(location)?.FullName;
            location = location.Substring(location.LastIndexOf('\\') + 1);
            string text;
            using (var process = ProcessUtil.StartProcess(_eac3toPath, location.WrapWithQuotes(), workingPath))
            {
                text = (await process.GetOutputDataAsync()).ToString();
            }
            if (text.Contains("HD DVD / Blu-Ray disc structure not found."))
            {
                Log.Debug("eac3to output:\n\n{Output}", text);
                throw new Exception("May be the path is too complex or directory contains nonAscii characters");
            }
            Log.Information("Disc Info:\n\n{Output}", text);

            var matched = new HashSet<string>();

            foreach (Match match in RDiskInfo.Matches(text))
            {
                var mpls = match.Groups["mpls"].Value;
                var dur = match.Groups["dur"].Value;
                if (!matched.Add(mpls)) continue;
                var item = new MPLSParser().Parse(Path.Combine(path, mpls)).CombineChapter();
                item.Data.Duration = TimeSpan.Parse(dur);
                list.Add(item);
            }
            return list;
        }
    }
}
