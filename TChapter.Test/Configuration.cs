// SPDX-License-Identifier: GPL-3.0-or-later
// SPDX-FileCopyrightText: Copyright 2017-2023 TautCony (i@tautcony.xyz)

using System;
using System.IO;
using System.Reflection;

namespace TChapter.Test
{
    static class Configuration
    {
        public static string BasePath { get; }
        public static string TestCaseBasePath { get; }

        public static Platform CurrentPlatform { get; }


        static Configuration()
        {
            BasePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var currPath = BasePath;
            do
            {
                currPath = Path.GetDirectoryName(currPath);
            } while (Path.GetFileName(currPath) != "TChapter.Test");

            TestCaseBasePath = Path.Combine(currPath, "Assets");

            CurrentPlatform = GetPlatform();
        }

        private static Platform GetPlatform()
        {
            var windir = Environment.GetEnvironmentVariable("windir");
            if (!string.IsNullOrEmpty(windir) && windir.Contains("\\") && Directory.Exists(windir))
            {
                return Platform.Windows;
            }

            if (File.Exists(@"/proc/sys/kernel/ostype"))
            {
                var osType = File.ReadAllText(@"/proc/sys/kernel/ostype");
                return osType.StartsWith("Linux", StringComparison.OrdinalIgnoreCase) ? Platform.Linux : Platform.Unknown;
            }
            return File.Exists(@"/System/Library/CoreServices/SystemVersion.plist") ? Platform.OSX : Platform.Unknown;
        }
    }

    public enum Platform
    {
        Windows,
        OSX,
        Linux,
        Unknown
    }
}
