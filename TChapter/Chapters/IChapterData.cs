// SPDX-License-Identifier: GPL-3.0-or-later
// SPDX-FileCopyrightText: Copyright 2017-2023 TautCony (i@tautcony.xyz)

using System.Collections.Generic;

namespace TChapter.Chapters
{
    public interface IChapterData : IList<ChapterInfo>
    {
        ChapterTypeEnum ChapterType { get; }

        object Source { get; }

        void Save(ChapterTypeEnum chapterType, string savePath, int index = 0, bool removeName = false,
            string language = "", string sourceFileName = "");
    }
}
