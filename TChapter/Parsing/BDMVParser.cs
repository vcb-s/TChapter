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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TChapter.Chapters;
using TChapter.Util;

namespace TChapter.Parsing
{
    public class BDMVParser : IChapterParser
    {
        public string _eac3toPath;

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
            var bdmvTitle = string.Empty;
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
                    var title = Regex.Match(xmlText, @"<di:name>(?<title>[^<]*)</di:name>");
                    if (title.Success)
                    {
                        bdmvTitle = title.Groups["title"].Value;
                        Logger.Log($"Disc Title: {bdmvTitle}");
                    }
                }
            }

            var workingPath = Directory.GetParent(location).FullName;
            location = location.Substring(location.LastIndexOf('\\') + 1);
            var text = (await ProcessUtil.RunProcessAsync(_eac3toPath, $"\"{location}\"", workingPath)).ToString();
            if (text.Contains("HD DVD / Blu-Ray disc structure not found."))
            {
                //Logger.Log(text);
                throw new Exception("May be the path is too complex or directory contains nonAscii characters");
            }
            Logger.Log("\r\nDisc Info:\r\n" + text);

            var matched = new HashSet<string>();

            foreach (Match match in RDiskInfo.Matches(text))
            {
                var mpls = match.Groups["mpls"].Value;
                var dur = match.Groups["dur"].Value;
                if (!matched.Add(mpls)) continue;
                var item = (new MPLSParser().Parse(Path.Combine(path, mpls)) as MultiChapterData)
                    .CombineChapter();
                item.Data.Duration = TimeSpan.Parse(dur);
                list.Add(item);
            }
            return list;
        }
    }
}