// ****************************************************************************
//
// Copyright (C) 2014 jirkapenzes (jirkapenzes@gmail.com)
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

namespace TChapter.Logging
{
    public class DebugLogger
    {
        private const Logger.Level DebugLevel = Logger.Level.Debug;

        public static void Log()
        {
            Log("There is no message");
        }

        public static void Log(string message)
        {
            Logger.Log(DebugLevel, message);
        }

        public static void Log(Exception exception)
        {
            Logger.Log(DebugLevel, exception?.Message ?? "NO MESSAGE");
        }

        public static void Log<TClass>(Exception exception) where TClass : class
        {
            var message = $"Log exception -> Message: {exception?.Message ?? "NO MESSAGE"}\nStackTrace: {exception?.StackTrace ?? "NO TRACE"}";
            Logger.Log<TClass>(DebugLevel, message);
        }

        public static void Log<TClass>(string message) where TClass : class
        {
            Logger.Log<TClass>(DebugLevel, message);
        }
    }
}
