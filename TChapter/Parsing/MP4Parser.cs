// ****************************************************************************
//
// Copyright (C) 2012 Jim Evans (james.h.evans.jr@gmail.com)
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
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using TChapter.Chapters;
using TChapter.Util;

namespace TChapter.Parsing
{
    public class MP4Parser : IChapterParser
    {
        public IChapterData Parse(string path)
        {
            var fileHandle = MP4ParserUtil.NativeMethods.MP4Read(path);
            if (fileHandle == IntPtr.Zero)
                throw new Exception("Failed to open the MP4 file");
            try
            {
                return ReadFromFile(fileHandle);
            }
            finally
            {
                MP4ParserUtil.NativeMethods.MP4Close(fileHandle);
            }
        }

        public IChapterData Parse(Stream stream)
        {
            throw new InvalidDataException();
        }

        /// <summary>
        /// Reads the chapter information from the specified file.
        /// </summary>
        /// <param name="fileHandle">The handle to the file from which to read the chapter information.</param>
        /// <returns>A new instance of a <see cref="List{T}"/> object containing the information
        /// about the chapters for the file.</returns>
        internal static IChapterData ReadFromFile(IntPtr fileHandle)
        {
            var chapter = new SingleChapterData(ChapterTypeEnum.MP4);
            var chapterListPointer = IntPtr.Zero;
            var chapterCount = 0;
            var chapterType = MP4ParserUtil.NativeMethods.MP4GetChapters(fileHandle, ref chapterListPointer, ref chapterCount, MP4ParserUtil.NativeMethods.MP4ChapterType.Any);
            Logger.Log($"Chapter type: {chapterType}");
            if (chapterType != MP4ParserUtil.NativeMethods.MP4ChapterType.None && chapterCount != 0)
            {
                var currentChapterPointer = chapterListPointer;
                var current = TimeSpan.Zero;
                for (var i = 0; i < chapterCount; ++i)
                {
                    var currentChapter = currentChapterPointer.ReadStructure<MP4ParserUtil.NativeMethods.MP4Chapter>();
                    var duration = TimeSpan.FromMilliseconds(currentChapter.duration);
                    var title = GetString(currentChapter.title);
                    Logger.Log($"{title} {duration}");
                    chapter.Chapters.Add(new Chapter {Time = current, Name = title, Number = i + 1});
                    current += duration;
                    currentChapterPointer = IntPtr.Add(currentChapterPointer, Marshal.SizeOf(currentChapter));
                }
            }
            else
            {
                var timeScale = MP4ParserUtil.NativeMethods.MP4GetTimeScale(fileHandle);
                var duration = MP4ParserUtil.NativeMethods.MP4GetDuration(fileHandle);
                chapter.Chapters.Add(new Chapter
                {
                    Time = TimeSpan.FromSeconds(duration / (double) timeScale),
                    Name = ChapterName.Get(1),
                    Number = 1
                });
            }
            if (chapterListPointer != IntPtr.Zero)
            {
                MP4ParserUtil.NativeMethods.MP4Free(chapterListPointer);
            }
            return chapter;
        }

        /// <summary>
        /// Decodes a C-Style string into a string, can handle UTF-8 or UTF-16 encoding.
        /// </summary>
        /// <param name="bytes">C-Style string</param>
        /// <returns></returns>
        private static string GetString(byte[] bytes)
        {
            if (bytes == null) return null;
            string title = null;
            if (bytes.Length <= 3) title = Encoding.UTF8.GetString(bytes);
            if (bytes[0] == 0xFF && bytes[1] == 0xFE) title = Encoding.Unicode.GetString(bytes);
            if (bytes[0] == 0xFE && bytes[1] == 0xFF) title = Encoding.BigEndianUnicode.GetString(bytes);
            if (bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF)
                title = new UTF8Encoding(false).GetString(bytes, 3, bytes.Length - 3);
            else if (title == null) title = Encoding.UTF8.GetString(bytes);

            return title.Substring(0, title.IndexOf('\0'));
        }
    }
}
