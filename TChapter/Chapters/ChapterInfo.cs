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
using System.Linq;
using TChapter.Objcet;
using TChapter.Util;

namespace TChapter.Chapters
{
    public class ChapterInfo
    {
        public string Title           { get; set; }
        /// <summary>
        /// Corresponding Video file
        /// </summary>
        public string SourceName      { get; set; }
        public double FramesPerSecond { get; set; }
        public TimeSpan Duration      { get; set; }
        public List<Chapter> Chapters { get; set; } = new List<Chapter>();
        public Expression Expr { get; set; } = Expression.Empty;

        public override string ToString() => $"{Title} - {Duration.Time2String()} - [{Chapters.Count} Chapters]";

        public void ChangeFps(double fps)
        {
            for (var i = 0; i < Chapters.Count; i++)
            {
                var c = Chapters[i];
                var frames = c.Time.TotalSeconds*FramesPerSecond;
                Chapters[i] = new Chapter
                {
                    Name = c.Name,
                    Time = new TimeSpan((long) Math.Round(frames/fps*TimeSpan.TicksPerSecond))
                };
            }
            var totalFrames = Duration.TotalSeconds*FramesPerSecond;
            Duration           = new TimeSpan((long) Math.Round(totalFrames/fps*TimeSpan.TicksPerSecond));
            FramesPerSecond    = fps;
        }

        #region updataInfo

        /// <summary>
        /// 以新的时间基准更新剩余章节
        /// </summary>
        /// <param name="shift">剩余章节的首个章节点的时间</param>
        public void UpdataInfo(TimeSpan shift)
        {
            Chapters.ForEach(item => item.Time -= shift);
        }

        /// <summary>
        /// 根据输入的数值向后位移章节序号
        /// </summary>
        /// <param name="shift">位移量</param>
        public void UpdataInfo(int shift)
        {
            var index = 0;
            Chapters.ForEach(item => item.Number = ++index + shift);
        }

        /// <summary>
        /// 根据给定的章节名模板更新章节
        /// </summary>
        /// <param name="chapterNameTemplate"></param>
        public void UpdataInfo(string chapterNameTemplate)
        {
            if (string.IsNullOrWhiteSpace(chapterNameTemplate)) return;
            using (var cn = chapterNameTemplate.Trim(' ', '\r', '\n').Split('\n').ToList().GetEnumerator()) //移除首尾多余空行
            {
                Chapters.ForEach(item => item.Name = cn.MoveNext() ? cn.Current : item.Name.Trim('\r')); //确保无多余换行符
            }
        }

        #endregion
    }
}
