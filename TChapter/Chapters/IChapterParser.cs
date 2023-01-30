// SPDX-License-Identifier: GPL-3.0-or-later
// SPDX-FileCopyrightText: Copyright 2017-2023 TautCony (i@tautcony.xyz)

using System.IO;

namespace TChapter.Chapters
{
    /// <summary>
    /// A contract for parsing chapter from file as an <see cref="IChapterData"/>.
    /// </summary>
    public interface IChapterParserFromFile
    {
        /// <summary>
        /// Parses a file into an <see cref="IChapterData"/>.
        /// </summary>
        /// <param name="path">The path of file to parse.</param>
        /// <returns>The parsed chapter.</returns>
        IChapterData Parse(string path);
    }

    /// <summary>
    /// A contract for parsing chapter from file as an <see cref="IChapterData"/>.
    /// </summary>
    public interface IChapterParserFromStream
    {
        /// <summary>
        /// Parses a stream into an <see cref="IChapterData"/>.
        /// </summary>
        /// <param name="stream">The stream to parse.</param>
        /// <returns>The parsed chapter.</returns>
        IChapterData Parse(Stream stream);
    }

    /// <summary>
    /// A contract for parsing chapter from different sources as an <see cref="IChapterData"/>.
    /// </summary>
    public interface IChapterParser : IChapterParserFromFile, IChapterParserFromStream
    {
    }
}
