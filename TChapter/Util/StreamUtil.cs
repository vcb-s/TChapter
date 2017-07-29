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
using System.IO;

namespace TChapter.Util
{
    public static class StreamUtil
    {
        public static byte[] ReadBytes(this Stream fs, int length)
        {
            var ret = new byte[length];
            fs.Read(ret, 0, length);
            return ret;
        }

        public static void Skip(this Stream fs, long length)
        {
            fs.Seek(length, SeekOrigin.Current);
            if (fs.Position > fs.Length)
                throw new Exception("Skip out of range");
        }

        #region int reader

        public static ulong BEInt64(this Stream fs)
        {
            var b = fs.ReadBytes(8);
            return b[7] + ((ulong)b[6] << 8) + ((ulong)b[5] << 16) + ((ulong)b[4] << 24) +
                ((ulong)b[3] << 32) + ((ulong)b[2] << 40) + ((ulong)b[1] << 48) + ((ulong)b[0] << 56);
        }

        public static uint BEInt32(this Stream fs)
        {
            var b = fs.ReadBytes(4);
            return b[3] + ((uint)b[2] << 8) + ((uint)b[1] << 16) + ((uint)b[0] << 24);
        }

        public static uint LEInt32(this Stream fs)
        {
            var b = fs.ReadBytes(4);
            return b[0] + ((uint)b[1] << 8) + ((uint)b[2] << 16) + ((uint)b[3] << 24);
        }

        public static int BEInt24(this Stream fs)
        {
            var b = fs.ReadBytes(3);
            return b[2] + (b[1] << 8) + (b[0] << 16);
        }

        public static int LEInt24(this Stream fs)
        {
            var b = fs.ReadBytes(3);
            return b[0] + (b[1] << 8) + (b[2] << 16);
        }

        public static int BEInt16(this Stream fs)
        {
            var b = fs.ReadBytes(2);
            return b[1] + (b[0] << 8);
        }

        public static int LEInt16(this Stream fs)
        {
            var b = fs.ReadBytes(2);
            return b[0] + (b[1] << 8);
        }
        #endregion
    }

    public class BitReader
    {
        private readonly byte[] _buffer;
        private int _bytePosition;
        private int _bitPositionInByte;

        public int Position => _bytePosition * 8 + _bitPositionInByte;

        public BitReader(byte[] source)
        {
            _buffer = new byte[source.Length];
            Array.Copy(source, _buffer, source.Length);
        }

        public void Reset()
        {
            _bytePosition = 0;
            _bitPositionInByte = 0;
        }

        public bool GetBit()
        {
            if (_bytePosition >= _buffer.Length)
                throw new IndexOutOfRangeException(nameof(_bytePosition));
            var ret = ((_buffer[_bytePosition] >> (7 - _bitPositionInByte)) & 1) == 1;
            Next();
            return ret;
        }

        private void Next()
        {
            ++_bitPositionInByte;
            if (_bitPositionInByte != 8) return;
            _bitPositionInByte = 0;
            ++_bytePosition;
        }

        public void Skip(int length)
        {
            for (var i = 0; i < length; ++i)
            {
                Next();
            }
        }

        public long GetBits(int length)
        {
            long ret = 0;
            for (var i = 0; i < length; ++i)
            {
                ret |= ((long)(_buffer[_bytePosition] >> (7 - _bitPositionInByte)) & 1) << (length - 1 - i);
                Next();
            }
            return ret;
        }
    }
}
