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
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using TChapter.Chapters;
using TChapter.Util;

namespace TChapter.Parsing
{
    public class OGMParser : IChapterParser
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
                return new SingleChapterData(ChapterTypeEnum.OGM)
                {
                    Data = GetChapterInfo(reader.ReadToEnd())
                };
            }
        }

        private static readonly Regex RTimeCodeLine = new Regex(@"^\s*CHAPTER\d+\s*=\s*(.*)", RegexOptions.Compiled);
        private static readonly Regex RNameLine = new Regex(@"^\s*CHAPTER\d+NAME\s*=\s*(?<chapterName>.*)", RegexOptions.Compiled);

        private enum LineState
        {
            LTimeCode,
            LName,
            LError,
            LFin
        }

        public static ChapterInfo GetChapterInfo(string text)
        {
            var index = 0;
            var info = new ChapterInfo();
            var lines = text.Trim(' ', '\t', '\r', '\n').Split('\n');
            var state = LineState.LTimeCode;
            TimeSpan timeCode = TimeSpan.Zero, initialTime;
            if (RTimeCodeLine.Match(lines.First()).Success)
            {
                initialTime = Config.TIME_FORMAT.Match(lines.First()).Value.ToTimeSpan();
            }
            else
            {
                throw new Exception($"ERROR: {lines.First()} <-Unmatched time format");
            }
            foreach (var line in lines)
            {
                switch (state)
                {
                    case LineState.LTimeCode:
                        if (string.IsNullOrWhiteSpace(line)) break; //跳过空行
                        if (RTimeCodeLine.Match(line).Success)
                        {
                            timeCode = Config.TIME_FORMAT.Match(line).Value.ToTimeSpan() - initialTime;
                            state = LineState.LName;
                            break;
                        }
                        state = LineState.LError;   //未获得预期的时间信息，中断解析
                        break;
                    case LineState.LName:
                        if (string.IsNullOrWhiteSpace(line)) break; //跳过空行
                        var name = RNameLine.Match(line);
                        if (name.Success)
                        {
                            info.Chapters.Add(new Chapter(name.Groups["chapterName"].Value.Trim('\r'), timeCode, ++index));
                            state = LineState.LTimeCode;
                            break;
                        }
                        state = LineState.LError;   //未获得预期的名称信息，中断解析
                        break;
                    case LineState.LError:
                        if (info.Chapters.Count == 0) throw new Exception("Unable to Parse this ogm file");
                        Logger.Log($"+Interrupt: Happened at [{line}]");    //将已解析的部分返回
                        state = LineState.LFin;
                        break;
                    case LineState.LFin:
                        goto EXIT_1;
                    default:
                        state = LineState.LError;
                        break;
                }
            }
            EXIT_1:
            info.Duration = info.Chapters.Last().Time;
            return info;
        }
    }
}