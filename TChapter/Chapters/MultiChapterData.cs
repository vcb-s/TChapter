// SPDX-License-Identifier: GPL-3.0-or-later
// SPDX-FileCopyrightText: Copyright 2017-2023 TautCony (i@tautcony.xyz)

using System;
using System.Collections.Generic;
using TChapter.Util;

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
            if (chapterData == null) return;
            foreach (var chapter in chapterData)
            {
                Add(chapter);
            }
        }

        public void Save(ChapterTypeEnum chapterType, string savePath, int index = 0, bool removeName = false, string language = "", string sourceFileName = "")
        {
            if (string.IsNullOrWhiteSpace(savePath))
                throw new Exception("The output path is empty");
            switch (chapterType)
            {
                case ChapterTypeEnum.CUE:
                    this.ToCUE(sourceFileName, index, removeName).SaveAs(savePath);
                    break;
                case ChapterTypeEnum.OGM:
                    this.ToOGM(index, removeName).SaveAs(savePath);
                    break;
                case ChapterTypeEnum.XML:
                    this.ToXML(language, index, removeName).SaveAs(savePath);
                    break;
                case ChapterTypeEnum.QPF:
                    this.ToQPFILE(index).SaveAs(savePath);
                    break;
                case ChapterTypeEnum.TIMECODES:
                    this.ToTIMECODES(index).SaveAs(savePath);
                    break;
                case ChapterTypeEnum.JSON:
                    this.ToJSON(index, removeName).SaveAs(savePath);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(chapterType), chapterType, null);
            }
        }
    }
}
