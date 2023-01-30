// SPDX-License-Identifier: GPL-3.0-or-later
// SPDX-FileCopyrightText: Copyright 2017-2023 TautCony (i@tautcony.xyz)

using System;
using System.Collections.Generic;
using System.Linq;
using TChapter.Object;
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
        public decimal FramesPerSecond { get; set; }
        public TimeSpan Duration      { get; set; }
        public List<Chapter> Chapters { get; set; } = new List<Chapter>();
        public Expression Expr        { get; set; } = Expression.Empty;

        public override string ToString() => $"{Title} - {Duration.Time2String()} - [{Chapters.Count} Chapters]";

        public void ChangeFps(decimal fps)
        {
            for (var i = 0; i < Chapters.Count; i++)
            {
                var c = Chapters[i];
                var frames = (decimal)c.Time.TotalSeconds * FramesPerSecond;
                Chapters[i] = new Chapter
                {
                    Name = c.Name,
                    Time = new TimeSpan((long)Math.Round(frames / fps * TimeSpan.TicksPerSecond))
                };
            }

            var totalFrames = (decimal)Duration.TotalSeconds * FramesPerSecond;
            Duration = new TimeSpan((long)Math.Round(totalFrames / fps * TimeSpan.TicksPerSecond));
            FramesPerSecond = fps;
        }

        #region updataInfo

        /// <summary>
        /// 以新的时间基准更新剩余章节
        /// </summary>
        /// <param name="shift">剩余章节的首个章节点的时间</param>
        public void UpdateInfo(TimeSpan shift)
        {
            Chapters.ForEach(item => item.Time -= shift);
        }

        /// <summary>
        /// 根据输入的数值向后位移章节序号
        /// </summary>
        /// <param name="shift">位移量</param>
        public void UpdateInfo(int shift)
        {
            var index = 0;
            Chapters.ForEach(item => item.Number = ++index + shift);
        }

        /// <summary>
        /// 根据给定的章节名模板更新章节
        /// </summary>
        /// <param name="chapterNameTemplate"></param>
        public void UpdateInfo(string chapterNameTemplate)
        {
            if (string.IsNullOrWhiteSpace(chapterNameTemplate)) return;
            using (var cn = chapterNameTemplate.Trim(' ', '\r', '\n').Split('\n').ToList().GetEnumerator()) //移除首尾多余空行
            {
                Chapters.ForEach(item => item.Name = cn.MoveNext() ? cn.Current : item.Name.Trim('\r')); //确保无多余换行符
            }
        }

        /// <summary>
        /// 若要使用帧数信息，则必须调用该方法，否则帧数信息为空
        /// </summary>
        /// <param name="index">帧数索引，见ChapterUtils.FrameRate</param>
        /// <param name="settingAccuracy">精确度阈值</param>
        /// <param name="round">是否帧数取整</param>
        public void UpdateFrameInfo(int index = 0, decimal settingAccuracy = 0.001M, bool round = true)
        {
            index = index == 0 ? GuessFps(settingAccuracy) : index;

            foreach (var chapter in Chapters)
            {
                var frames = Expr.Eval(chapter.Time.TotalSeconds, FramesPerSecond) * ChapterUtil.FrameRate[index];
                if (round)
                {
                    var rounded = Math.Round(frames, MidpointRounding.AwayFromZero);
                    var accuracy = Math.Abs(frames - rounded) < settingAccuracy;
                    chapter.FramesInfo = $"{rounded}{(accuracy ? " K" : " *")}";
                }
                else
                {
                    chapter.FramesInfo = $"{frames}";
                }
            }
        }

        private int GuessFps(decimal accuracy)
        {
            var result = ChapterUtil.FrameRate.Select(fps =>
                        this.Chapters.Sum(item =>
                        item.IsAccuracy(fps, accuracy, this.Expr))).ToList();
            result[0] = 0; // skip two invalid frame rate.
            result[5] = 0;
            var autofpsCode = result.IndexOf(result.Max());
            this.FramesPerSecond = ChapterUtil.FrameRate[autofpsCode];
            return autofpsCode == 0 ? 1 : autofpsCode;
        }

        #endregion

        public SingleChapterData ToChapterData()
        {
            return new SingleChapterData(ChapterTypeEnum.UNKNOWN)
            {
                Data = this
            };
        }
    }
}
