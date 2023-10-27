// SPDX-License-Identifier: GPL-3.0-or-later
// SPDX-FileCopyrightText: Copyright 2017-2023 TautCony (i@tautcony.xyz)

using System.Text.RegularExpressions;

namespace TChapter
{
    public static class Config
    {
        public const string DEFAULT_CHAPTER_NAME = "Chapter";

        public static readonly Regex TIME_FORMAT = new Regex(@"(?<Hour>\d+)\s*:\s*(?<Minute>\d+)\s*:\s*(?<Second>\d+)\s*[\.,]\s*(?<Millisecond>\d{3,9})", RegexOptions.Compiled);

        public static readonly decimal[] FRAME_RATE = { 0M, 24000M / 1001, 24M, 25M, 30000M / 1001, 0M, 50M, 60000M / 1001 };
    }
}
