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
using System.Runtime.InteropServices;

namespace TChapter.Parsing
{
    internal static class MP4ParserUtil
    {
        /// <summary>
        /// Contains methods used for interfacing with the native code MP4V2 library.
        /// </summary>
        internal static class NativeMethods
        {
            /// <summary>
            /// Represents the known types used for chapters.
            /// </summary>
            /// <remarks>
            /// These values are taken from the MP4V2 header files, documented thus:
            /// <para>
            /// <code>
            /// typedef enum {
            ///     MP4ChapterTypeNone = 0,
            ///     MP4ChapterTypeAny  = 1,
            ///     MP4ChapterTypeQt   = 2,
            ///     MP4ChapterTypeNero = 4
            /// } MP4ChapterType;
            /// </code>
            /// </para>
            /// </remarks>
            internal enum MP4ChapterType
            {
                /// <summary>
                /// No chapters found return value
                /// </summary>
                None = 0,

                /// <summary>
                /// Any or all known chapter types
                /// </summary>
                Any = 1,

                /// <summary>
                /// QuickTime chapter type
                /// </summary>
                Qt = 2,

                /// <summary>
                /// Nero chapter type
                /// </summary>
                Nero = 4
            }

            [DllImport("libMP4V2.dll", CharSet = CharSet.Ansi, ExactSpelling = true, BestFitMapping = false, SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
            internal static extern IntPtr MP4Read([MarshalAs(UnmanagedType.LPStr)]string fileName);

            [DllImport("libMP4V2.dll", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
            internal static extern void MP4Close(IntPtr file);

            [DllImport("libMP4V2.dll", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
            internal static extern void MP4Free(IntPtr pointer);

            [DllImport("libMP4V2.dll", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
            [return: MarshalAs(UnmanagedType.I4)]
            internal static extern MP4ChapterType MP4GetChapters(IntPtr hFile, ref IntPtr chapterList, ref int chapterCount, MP4ChapterType chapterType);

            [DllImport("libMP4V2.dll", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
            internal static extern long MP4GetDuration(IntPtr hFile);

            [DllImport("libMP4V2.dll", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
            internal static extern int MP4GetTimeScale(IntPtr hFile);

            /// <summary>
            /// Represents information for a chapter in this file.
            /// </summary>
            /// <remarks>
            /// This structure definition is taken from the MP4V2 header files, documented thus:
            /// <para>
            /// <code>
            /// #define MP4V2_CHAPTER_TITLE_MAX 1023
            ///
            /// typedef struct MP4Chapter_s {
            ///     MP4Duration duration;
            ///     char title[MP4V2_CHAPTER_TITLE_MAX+1];
            /// } MP4Chapter_t;
            /// </code>
            /// </para>
            /// </remarks>
            [StructLayout(LayoutKind.Sequential)]
            internal struct MP4Chapter
            {
                /// <summary>
                /// Duration of chapter in milliseconds
                /// </summary>
                internal long duration;

                /// <summary>
                /// Title of chapter
                /// </summary>
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1024)]
                internal byte[] title;
            }
        }
    }

    /// <summary>
    /// The <see cref="IntPtrExtensions"/> class contains extension methods used
    /// for marshaling data between managed and unmanaged code.
    /// </summary>
    public static class IntPtrExtensions
    {
        /// <summary>
        /// Reads a structure beginning at the location pointed to in memory by the
        /// specified pointer value.
        /// </summary>
        /// <typeparam name="T">The type of the structure to read.</typeparam>
        /// <param name="value">The <see cref="IntPtr"/> value indicating the location
        /// in memory at which to begin reading data.</param>
        /// <returns>An instance of the specified structure type.</returns>
        /// <exception cref="ArgumentNullException">Thrown when this <see cref="IntPtr"/>
        /// is a null pointer (<see cref="IntPtr.Zero"/>).</exception>
        public static T ReadStructure<T>(this IntPtr value)
        {
            if (value == IntPtr.Zero)
            {
                throw new ArgumentNullException(nameof(value), "Structures cannot be read from a null pointer (IntPtr.Zero)");
            }

            return (T)Marshal.PtrToStructure(value, typeof(T));
        }
    }
}
