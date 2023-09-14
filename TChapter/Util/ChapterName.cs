// SPDX-License-Identifier: GPL-3.0-or-later
// SPDX-FileCopyrightText: Copyright 2017-2023 TautCony (i@tautcony.xyz)

using System;
using System.Collections.Generic;

namespace TChapter.Util
{
    public class ChapterName
    {
        public int Index { get; private set; }

        private const string ChapterFormat = Config.DEFAULT_CHAPTER_NAME;

        public static Func<string> GetChapterName(string chapterFormat = ChapterFormat)
        {
            var index = 1;
            return () => $"{chapterFormat} {index++:D2}";
        }

        public ChapterName(int index) => Index = index;

        public ChapterName() => Index = 1;

        public void Reset() => Index = 1;

        public string Get() => $"{ChapterFormat} {Index++:D2}";

        public static string Get(int index) => $"{ChapterFormat} {index:D2}";

        /// <summary>
        /// 生成指定范围内的标准章节名的序列
        /// </summary>
        /// <param name="start"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static IEnumerable<string> Range(int start, int count)
        {
            if (start < 0 || start > 99) throw new ArgumentOutOfRangeException(nameof(start));
            var max = start + count - 1;
            if (count < 0 || max > 99) throw new ArgumentOutOfRangeException(nameof(count));
            return RangeIterator(start, count);
        }

        private static IEnumerable<string> RangeIterator(int start, int count)
        {
            for (var i = 0; i < count; i++) yield return $"{ChapterFormat} {start + i:D2}";
        }
    }
}
