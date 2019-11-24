// ****************************************************************************
//
// Copyright (C) 2009-2015 Kurtnoise (kurtnoise@free.fr)
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

namespace TChapter.Parsing
{
    internal static class IFOParserUtil
    {
        internal static byte[] GetFileBlock(this FileStream ifoStream, long position, int count)
        {
            if (position < 0) throw new Exception("Invalid Ifo file");
            var buf = new byte[count];
            ifoStream.Seek(position, SeekOrigin.Begin);
            ifoStream.Read(buf, 0, count);
            return buf;
        }

        private static byte? GetFrames(byte value)
        {
            if (((value >> 6) & 0x01) == 1) //check whether the second bit of value is 1
            {
                return (byte)BcdToInt((byte)(value & 0x3F)); //only last 6 bits is in use, show as BCD code
            }
            return null;
        }

        internal static long GetPCGIP_Position(this FileStream ifoStream)
        {
            return ToFilePosition(ifoStream.GetFileBlock(0xCC, 4));
        }

        internal static int GetProgramChains(this FileStream ifoStream, long pcgitPosition)
        {
            return ToInt16(ifoStream.GetFileBlock(pcgitPosition, 2));
        }

        internal static uint GetChainOffset(this FileStream ifoStream, long pcgitPosition, int programChain)
        {
            return ToInt32(ifoStream.GetFileBlock((pcgitPosition + (8 * programChain)) + 4, 4));
        }

        internal static int GetNumberOfPrograms(this FileStream ifoStream, long pcgitPosition, uint chainOffset)
        {
            return ifoStream.GetFileBlock((pcgitPosition + chainOffset) + 2, 1)[0];
        }

        internal static IfoTimeSpan? ReadTimeSpan(this FileStream ifoStream, long pcgitPosition, uint chainOffset, out bool isNTSC)
        {
            return ReadTimeSpan(ifoStream.GetFileBlock((pcgitPosition + chainOffset) + 4, 4), out isNTSC);
        }

        /// <param name="playbackBytes">
        /// byte[0] hours in bcd format<br/>
        /// byte[1] minutes in bcd format<br/>
        /// byte[2] seconds in bcd format<br/>
        /// byte[3] milliseconds in bcd format (2 high bits are the frame rate)
        /// </param>
        /// <param name="isNTSC">frame rate mode of the chapter</param>
        internal static IfoTimeSpan? ReadTimeSpan(byte[] playbackBytes, out bool isNTSC)
        {
            var frames = GetFrames(playbackBytes[3]);
            var fpsMask = playbackBytes[3] >> 6;
            //var fps = fpsMask == 0x01 ? 25M : fpsMask == 0x03 ? (30M / 1.001M) : 0;
            isNTSC = fpsMask == 0x03;
            if (frames == null) return null;
            try
            {
                var hours = BcdToInt(playbackBytes[0]);
                var minutes = BcdToInt(playbackBytes[1]);
                var seconds = BcdToInt(playbackBytes[2]);
                return new IfoTimeSpan(hours, minutes, seconds, (int) frames, isNTSC);
            }
            catch (Exception exception)
            {
                Logger.Log(exception);
                return null;
            }
        }

        /// <summary>
        /// get number of PGCs
        /// </summary>
        /// <param name="fileName">name of the IFO file</param>
        /// <returns>number of PGS as an integer</returns>
        public static int GetPGCnb(string fileName)
        {
            var ifoStream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            var offset = ToInt32(GetFileBlock(ifoStream, 0xCC, 4));    // Read PGC offset
            ifoStream.Seek(2048 * offset + 0x01, SeekOrigin.Begin);               // Move to beginning of PGC
            //long VTS_PGCITI_start_position = ifoStream.Position - 1;
            var nPGCs = ifoStream.ReadByte();       // Number of PGCs
            ifoStream.Close();
            return nPGCs;
        }

        internal static short ToInt16(byte[] bytes) => (short)((bytes[0] << 8) + bytes[1]);
        private static uint ToInt32(byte[] bytes) => (uint)((bytes[0] << 24) + (bytes[1] << 16) + (bytes[2] << 8) + bytes[3]);
        public static int BcdToInt(byte value) => (0xFF & (value >> 4)) * 10 + (value & 0x0F);

        private static long ToFilePosition(byte[] bytes) => ToInt32(bytes) * 0x800L;
    }

    public struct IfoTimeSpan : IEquatable<TimeSpan>
    {
        public long TotalFrames { get; set; }
        public bool IsNTSC { get; set; }

        public int RawFrameRate => IsNTSC ? 30 : 25;
        private decimal TimeFrameRate => IsNTSC ? 30000M / 1001 : 25;

        public int Hours => (int)Math.Round(TotalFrames / TimeFrameRate / 3600);
        public int Minutes => (int)Math.Round(TotalFrames / TimeFrameRate / 60) % 60;
        public int Second => (int)Math.Round(TotalFrames / TimeFrameRate) % 60;

        public static readonly IfoTimeSpan Zero = new IfoTimeSpan(true);

        public IfoTimeSpan(bool isNTSC)
        {
            TotalFrames = 0;
            IsNTSC = isNTSC;
        }

        private IfoTimeSpan(long totalFrames, bool isNTSC)
        {
            IsNTSC = isNTSC;
            TotalFrames = totalFrames;
        }

        public IfoTimeSpan(int seconds, int frames, bool isNTSC)
        {
            IsNTSC = isNTSC;
            TotalFrames = frames;
            TotalFrames += seconds * RawFrameRate;
        }

        public IfoTimeSpan(int hour, int minute, int second, int frames, bool isNTSC)
        {
            IsNTSC = isNTSC;
            TotalFrames = frames;
            TotalFrames += (hour * 3600 + minute * 60 + second) * RawFrameRate;
        }

        public IfoTimeSpan(TimeSpan time, bool isNTSC)
        {
            IsNTSC = isNTSC;
            TotalFrames = 0;
            TotalFrames = (long)Math.Round((decimal)time.TotalSeconds / TimeFrameRate);
        }

        public static implicit operator TimeSpan(IfoTimeSpan time)
        {
            return new TimeSpan((long)Math.Round(time.TotalFrames / time.TimeFrameRate * TimeSpan.TicksPerSecond));
        }

        public TimeSpan ToTimeSpan()
        {
            return this;
        }

        #region Operator
        private static void FrameRateModeCheck(IfoTimeSpan t1, IfoTimeSpan t2)
        {
            if (t1.IsNTSC ^ t2.IsNTSC)
                throw new InvalidOperationException("Unmatched frames rate mode");
        }


        public static IfoTimeSpan Add(IfoTimeSpan left, IfoTimeSpan right)
        {
            FrameRateModeCheck(left, right);
            return new IfoTimeSpan(left.TotalFrames + right.TotalFrames, left.IsNTSC);
        }

        public static IfoTimeSpan Subtract(IfoTimeSpan left, IfoTimeSpan right)
        {
            FrameRateModeCheck(left, right);
            return new IfoTimeSpan(left.TotalFrames - right.TotalFrames, left.IsNTSC);
        }

        public int CompareTo(IfoTimeSpan other)
        {
            FrameRateModeCheck(this, other);
            var diff = TotalFrames - other.TotalFrames;
            return diff == 0 ? 0 : diff < 0 ? -1 : 1;
        }


        public static IfoTimeSpan operator +(IfoTimeSpan left, IfoTimeSpan right)
        {
            return Add(left, right);
        }

        public static IfoTimeSpan operator -(IfoTimeSpan left, IfoTimeSpan right)
        {
            return Subtract(left, right);
        }

        public static bool operator <(IfoTimeSpan left, IfoTimeSpan right)
        { 
            return left.CompareTo(right) < 0;
        }

        public static bool operator >(IfoTimeSpan left, IfoTimeSpan right)
        {
            return left.CompareTo(right) > 0;
        }

        public static bool operator ==(IfoTimeSpan left, IfoTimeSpan right)
        {
            return left.CompareTo(right) == 0;
        }

        public static bool operator !=(IfoTimeSpan left, IfoTimeSpan right)
        {
            return !(left == right);
        }

        #endregion

        public override int GetHashCode()
        {
            return ((TotalFrames << 1) | (IsNTSC ? 1L : 0L)).GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (obj.GetType() != GetType())
                return false;
            var time = (IfoTimeSpan)obj;
            return TotalFrames == time.TotalFrames && IsNTSC == time.IsNTSC;
        }

        public override string ToString()
        {
            return $"{Hours:D2}:{Minutes:D2}:{Second:D2}.{TotalFrames} [{(IsNTSC ? 'N' : 'P')}]";
        }

        bool IEquatable<TimeSpan>.Equals(TimeSpan other)
        {
            var tick = (long) Math.Round(TotalFrames / TimeFrameRate * TimeSpan.TicksPerSecond);
            return tick == other.Ticks;
        }
    }
}
