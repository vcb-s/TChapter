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
