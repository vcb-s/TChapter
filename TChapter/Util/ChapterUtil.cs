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
using System.Text;
using TChapter.Chapters;
using TChapter.Object;

namespace TChapter.Util
{
    public static class ChapterUtil
    {
        public static readonly decimal[] FrameRate = { 0M, 24000M / 1001, 24M, 25M, 30000M / 1001, 0M, 50M, 60000M / 1001 };

        /// <summary>
        /// 将TimeSpan对象转换为 hh:mm:ss.sss 形式的字符串
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public static string Time2String(this TimeSpan time)
        {
            var millisecond = (int) Math.Round((time.TotalSeconds - Math.Floor(time.TotalSeconds)) * 1000);
            return $"{time.Hours:D2}:{time.Minutes:D2}:{time.Seconds:D2}.{millisecond:D3}";
        }


        /// <summary>
        /// 将给定的章节点时间以平移、修正信息修正后转换为 hh:mm:ss.sss 形式的字符串
        /// </summary>
        /// <param name="item">章节点</param>
        /// <param name="expr">转换表达式</param>
        /// <returns></returns>
        public static string Time2String(this Chapter item, Expression expr) => new TimeSpan(
            (long) (expr.Eval(item.Time.TotalSeconds) * TimeSpan.TicksPerSecond)).Time2String();


        /// <summary>
        /// 将符合 hh:mm:ss.sss 形式的字符串转换为TimeSpan对象
        /// </summary>
        /// <param name="input">时间字符串</param>
        /// <returns></returns>
        public static TimeSpan ToTimeSpan(this string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return TimeSpan.Zero;
            var        timeMatch = Config.TIME_FORMAT.Match(input);
            if (!timeMatch.Success) return TimeSpan.Zero;
            var hour        = int.Parse(timeMatch.Groups["Hour"].Value);
            var minute      = int.Parse(timeMatch.Groups["Minute"].Value);
            var second      = int.Parse(timeMatch.Groups["Second"].Value);
            var millisecond = int.Parse(timeMatch.Groups["Millisecond"].Value);
            return new TimeSpan(0, hour, minute, second, millisecond);
        }

        public static string ToCueTimeStamp(this TimeSpan input)
        {
            var frames = (int) Math.Round(input.Milliseconds*75/1000F);
            if (frames > 99) frames = 99;
            return $"{input.Hours*60 + input.Minutes:D2}:{input.Seconds:D2}:{frames:D2}";
        }

        /// <summary>
        /// 根据给定的帧率返回它在FrameRate表中的序号
        /// </summary>
        /// <param name="frame"></param>
        /// <returns></returns>
        public static int ConvertFr2Index(double frame)
        {
            for (var i = 0; i < Config.FRAME_RATE.Length; ++i)
            {
                if (Math.Abs(frame - (double)Config.FRAME_RATE[i]) < 1e-5)
                    return i;
            }
            return 0;
        }

        /// <summary>
        /// 将分开多段的章节合并为一个章节
        /// </summary>
        /// <param name="source">解析获得的分段章节</param>
        /// <returns></returns>
        public static SingleChapterData CombineChapter(this IChapterData source)
        {
            var fullChapter = new SingleChapterData(source.ChapterType)
            {
                Data = {Title = "Full_Chapter", FramesPerSecond = source.First().FramesPerSecond},
            };
            var duration = TimeSpan.Zero;
            var name = new ChapterName();
            foreach (var chapterClip in source)
            {
                foreach (var item in chapterClip.Chapters)
                {
                    fullChapter.Chapters.Add(new Chapter
                    {
                        Time = duration + item.Time,
                        Number = name.Index,
                        Name = name.Get()
                    });
                }
                duration += chapterClip.Duration; //每次加上当前段的总时长作为下一段位移的基准
            }
            fullChapter.Data.Duration = duration;
            return fullChapter;
        }

        public static void SaveAs(this string[] chapter, string path) => File.WriteAllLines(path, chapter, Encoding.UTF8);

        public static void SaveAs(this string chapter, string path) => File.WriteAllText(path, chapter, Encoding.UTF8);

        public static void SaveAs(this object chapter, string path) => File.WriteAllText(path, chapter?.ToString() ?? "", Encoding.UTF8);
    }
}
