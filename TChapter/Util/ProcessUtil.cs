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
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TChapter.Util
{
    public static class ProcessUtil
    {
        public static async Task<StringBuilder> RunProcessAsync(string fileName, string args, string workingDirectory = "")
        {
            using (var process = new Process
            {
                StartInfo =
                {
                    FileName = fileName, Arguments = args,
                    UseShellExecute = false, CreateNoWindow = true,
                    RedirectStandardOutput = true, RedirectStandardError = true
                },
                EnableRaisingEvents = true
            })
            {
                if (!string.IsNullOrEmpty(workingDirectory))
                {
                    process.StartInfo.WorkingDirectory = workingDirectory;
                }
                return await RunProcessAsync(process).ConfigureAwait(false);
            }
        }

        private static Task<StringBuilder> RunProcessAsync(Process process)
        {
            var tcs = new TaskCompletionSource<StringBuilder>();
            var ret = new StringBuilder();
            process.Exited += (sender, args) => tcs.SetResult(ret);
            process.OutputDataReceived += (sender, args) => ret.AppendLine(args.Data?.Trim('\b', ' '));
            //process.ErrorDataReceived += (s, ea) => Debug.WriteLine("ERR: " + ea.Data);

            if (!process.Start())
            {
                throw new InvalidOperationException("Could not start process: " + process);
            }

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            return tcs.Task;
        }

        /// <summary>
        /// Creates a DataReceivedEventArgs instance with the given Data.
        /// </summary>
        /// <param name="argData"></param>
        /// <returns></returns>
        public static DataReceivedEventArgs GetDataReceivedEventArgs(object argData)
        {
            var eventArgs = (DataReceivedEventArgs)System.Runtime.Serialization.FormatterServices.GetUninitializedObject(typeof(DataReceivedEventArgs));
            var fileds = typeof(DataReceivedEventArgs).GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)[0];
            fileds.SetValue(eventArgs, argData);

            return eventArgs;
        }

        /// <summary>
        /// Reads a Process's standard output stream character by character and calls the user defined method for each line
        /// </summary>
        /// <param name="argProcess"></param>
        /// <param name="argHandler"></param>
        public static void ReadStreamPerCharacter(Process argProcess, DataReceivedEventHandler argHandler)
        {
            var reader = argProcess.StandardOutput;
            var line = new StringBuilder();
            while (!reader.EndOfStream)
            {
                var c = (char)reader.Read();
                switch (c)
                {
                    case '\r':
                        if ((char)reader.Peek() == '\n') reader.Read();// consume the next character
                        argHandler(argProcess, GetDataReceivedEventArgs(line.ToString()));
                        line.Clear();
                        break;
                    case '\n':
                        argHandler(argProcess, GetDataReceivedEventArgs(line.ToString()));
                        line.Clear();
                        break;
                    default:
                        line.Append(c);
                        break;
                }
            }
        }
    }
}