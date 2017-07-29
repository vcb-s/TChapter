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

namespace TChapter.Chapters
{
    public class MultiChapterData : List<ChapterInfo>, IChapterData
    {
        public ChapterTypeEnum ChapterType { get; protected set; }
        public object Source { get; protected set; } = default(object);

        public MultiChapterData(ChapterTypeEnum chapterType)
        {
            ChapterType = chapterType;
        }

        public void Add(MultiChapterData chapterData)
        {
            foreach (var chapter in chapterData)
            {
                Add(chapter);
            }
        }

        public void Save(ChapterTypeEnum chapterType, int index = 0)
        {
            switch (chapterType)
            {
                case ChapterTypeEnum.CUE:

                    break;

                case ChapterTypeEnum.OGM:
                    break;

                case ChapterTypeEnum.XML:
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(chapterType), chapterType, null);
            }
        }
    }
}