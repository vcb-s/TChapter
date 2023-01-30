// SPDX-License-Identifier: GPL-3.0-or-later
// SPDX-FileCopyrightText: Copyright 2017-2023 TautCony (i@tautcony.xyz)

using System;
using TChapter.Object;
using TChapter.Util;

namespace TChapter.Chapters
{
    public class Chapter
    {
        /// <summary>Chapter Number</summary>
        public int Number       { get; set; }
        /// <summary>Chapter TimeStamp</summary>
        public TimeSpan Time    { get; set; }
        /// <summary>Chapter Name</summary>
        public string Name      { get; set; }
        /// <summary>Frame Count</summary>
        public string FramesInfo { get; set; } = string.Empty;
        public override string ToString() => $"{Name} - {Time.Time2String()}";

        public Chapter() { }

        public Chapter(string name, TimeSpan time, int number)
        {
            Number = number;
            Time   = time;
            Name   = name;
        }

        public int IsAccuracy(decimal fps, decimal accuracy, Expression expr = null)
        {
            var frames = (decimal) Time.TotalMilliseconds * fps / 1000M;
            if (expr != null) frames = expr.Eval(Time.TotalSeconds) * fps;
            var rounded = Math.Round(frames, MidpointRounding.AwayFromZero);
            return Math.Abs(frames - rounded) < accuracy ? 1 : 0;
        }
    }
}
