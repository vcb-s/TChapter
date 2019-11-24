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
using System.Linq;
using System.Text.RegularExpressions;
using TChapter.Chapters;

namespace TChapter.Parsing
{
    public class CUEParser: IChapterParser
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
                return ParseCue(reader.ReadToEnd());
            }
            
        }

        private enum NextState
        {
            Start,
            NewTrack,
            Track,
            Error,
            Fin
        }

        private static readonly Regex RTitle = new Regex(@"TITLE\s+\""(.+)\""", RegexOptions.Compiled);
        private static readonly Regex RFile = new Regex(@"FILE\s+\""(.+)\""\s+(WAVE|MP3|AIFF|BINARY|MOTOROLA)", RegexOptions.Compiled);
        private static readonly Regex RTrack = new Regex(@"TRACK\s+(\d+)", RegexOptions.Compiled);
        private static readonly Regex RPerformer = new Regex(@"PERFORMER\s+\""(.+)\""", RegexOptions.Compiled);
        private static readonly Regex RTime = new Regex(@"INDEX\s+(?<index>\d+)\s+(?<M>\d{2}):(?<S>\d{2}):(?<F>\d{2})", RegexOptions.Compiled);

        /// <summary>
        /// 解析 cue 播放列表
        /// </summary>
        /// <param name="context">未分行的cue字符串</param>
        /// <returns></returns>
        private static SingleChapterData ParseCue(string context)
        {
            var lines = context.Split('\n');
            var cue = new SingleChapterData(ChapterTypeEnum.CUE);
            var nxState = NextState.Start;
            Chapter chapter = null;

            foreach (var line in lines)
            {
                switch (nxState)
                {
                    case NextState.Start:
                        var chapterTitleMatch = RTitle.Match(line);
                        var fileMatch = RFile.Match(line);
                        if (chapterTitleMatch.Success)
                        {
                            cue.Data.Title = chapterTitleMatch.Groups[1].Value;
                            //nxState   = NextState.NewTrack;
                            break;
                        }
                        if (fileMatch.Success)          //Title 为非必需项，故当读取到File行时跳出
                        {
                            cue.Data.SourceName = fileMatch.Groups[1].Value;
                            nxState = NextState.NewTrack;
                        }
                        break;

                    case NextState.NewTrack:
                        if (string.IsNullOrWhiteSpace(line))    //读到空行，解析终止
                        {
                            nxState = NextState.Fin;
                            break;
                        }
                        var trackMatch = RTrack.Match(line);
                        if (trackMatch.Success)         //读取到Track，获取其编号，跳至下一步
                        {
                            chapter = new Chapter { Number = int.Parse(trackMatch.Groups[1].Value) };
                            nxState = NextState.Track;
                        }
                        break;

                    case NextState.Track:
                        var trackTitleMatch = RTitle.Match(line);
                        var performerMatch = RPerformer.Match(line);
                        var timeMatch = RTime.Match(line);

                        if (trackTitleMatch.Success)    //获取章节名
                        {
                            Debug.Assert(chapter != null);
                            chapter.Name = trackTitleMatch.Groups[1].Value.Trim('\r');
                            break;
                        }
                        if (performerMatch.Success)     //获取艺术家名
                        {
                            Debug.Assert(chapter != null);
                            chapter.Name += $" [{performerMatch.Groups[1].Value.Trim('\r')}]";
                            break;
                        }
                        if (timeMatch.Success)          //获取章节时间
                        {
                            var trackIndex = int.Parse(timeMatch.Groups["index"].Value);
                            switch (trackIndex)
                            {
                                case 0: //pre-gap of a track, just ignore it.
                                    break;

                                case 1: //beginning of a new track.
                                    Debug.Assert(chapter != null);
                                    var minute = int.Parse(timeMatch.Groups["M"].Value);
                                    var second = int.Parse(timeMatch.Groups["S"].Value);
                                    var millisecond = (int)Math.Round(int.Parse(timeMatch.Groups["F"].Value) * (1000F / 75));//最后一项以帧(1s/75)而非以10毫秒为单位
                                    chapter.Time = new TimeSpan(0, 0, minute, second, millisecond);
                                    cue.Chapters.Add(chapter);
                                    nxState = NextState.NewTrack;//当前章节点的必要信息已获得，继续寻找下一章节
                                    break;

                                default:
                                    nxState = NextState.Error;
                                    break;
                            }
                        }
                        break;

                    case NextState.Error:
                        throw new Exception("Unable to Parse this cue file");
                    case NextState.Fin:
                        goto EXIT_1;
                    default:
                        nxState = NextState.Error;
                        break;
                }
            }
            EXIT_1:
            if (cue.Chapters.Count < 1)
            {
                throw new Exception("Empty cue file");
            }
            cue.Chapters.Sort((c1, c2) => c1.Number.CompareTo(c2.Number));//确保无乱序
            cue.Data.Duration = cue.Chapters.Last().Time;
            return cue;
        }
    }
}