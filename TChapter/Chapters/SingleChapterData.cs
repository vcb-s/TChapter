// SPDX-License-Identifier: GPL-3.0-or-later
// SPDX-FileCopyrightText: Copyright 2017-2023 TautCony (i@tautcony.xyz)

using System.Collections.Generic;

namespace TChapter.Chapters
{
    public class SingleChapterData : MultiChapterData
    {
        public ChapterInfo Data
        {
            get => Count == 0 ? null : this[0];
            set
            {
                if (Count == 0) Add(value);
                else this[0] = value;
            }
        }

        public List<Chapter> Chapters => Data.Chapters;

        public SingleChapterData(ChapterTypeEnum chapterType) : base(chapterType)
        {
            Add(new ChapterInfo());
        }
    }
}
