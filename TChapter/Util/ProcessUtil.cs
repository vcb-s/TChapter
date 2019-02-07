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
        public static string WrapWithQuotes(this string str) => $"\"{str}\"";

        public static Process StartProcess(string fileName, string args, string workingDirectory = "")
        {
            var process = new Process
            {
                StartInfo =
                {
                    FileName = fileName,
                    Arguments = args,
                    WorkingDirectory = workingDirectory,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                }
            };
            if (!process.Start()) throw new InvalidOperationException($"Could not start process: {process}");
            return process;
        }

        public static Task<StringBuilder> GetOutputDataAsync(this Process process)
        {
            if (process == null) throw new ArgumentNullException(nameof(process));
            if (process.HasExited) return Task.FromResult(new StringBuilder());

            var ret = new StringBuilder();
            var tcs = new TaskCompletionSource<StringBuilder>();

            process.EnableRaisingEvents = true;
            process.Exited += OnProcessExited;
            process.OutputDataReceived += (sender, args) => ret.AppendLine(args.Data?.Trim('\b', ' '));

            process.BeginOutputReadLine();

            return tcs.Task;

            void OnProcessExited(object sender, EventArgs e)
            {
                process.Exited -= OnProcessExited;
                tcs.SetResult(ret);
            }
        }

        public static Task<StringBuilder> GetErrorDataAsync(this Process process)
        {
            if (process == null) throw new ArgumentNullException(nameof(process));
            if (process.HasExited) return Task.FromResult(new StringBuilder());

            var ret = new StringBuilder();
            var tcs = new TaskCompletionSource<StringBuilder>();

            process.EnableRaisingEvents = true;
            process.Exited += OnProcessExited;
            process.ErrorDataReceived += (sender, args) => ret.AppendLine(args.Data?.Trim('\b', ' '));

            process.BeginErrorReadLine();

            return tcs.Task;

            void OnProcessExited(object sender, EventArgs e)
            {
                process.Exited -= OnProcessExited;
                tcs.SetResult(ret);
            }
        }

        public static Task<int> WaitForExitAsync(this Process process)
        {
            if (process == null) throw new ArgumentNullException(nameof(process));
            if (process.HasExited) return Task.FromResult(0);

            var tcs = new TaskCompletionSource<int>();

            process.EnableRaisingEvents = true;
            process.Exited += OnProcessExited;

            return tcs.Task;

            void OnProcessExited(object sender, EventArgs e)
            {
                process.Exited -= OnProcessExited;
                tcs.SetResult(process.ExitCode);
            }
        }

        /// <summary>
        /// Creates a DataReceivedEventArgs instance with the given Data.
        /// </summary>
        /// <param name="argData"></param>
        /// <returns></returns>
        public static DataReceivedEventArgs GetDataReceivedEventArgs(object argData)
        {
            var eventArgs = (DataReceivedEventArgs)System.Runtime.Serialization.FormatterServices.GetUninitializedObject(typeof(DataReceivedEventArgs));
            var fields = typeof(DataReceivedEventArgs).GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)[0];
            fields.SetValue(eventArgs, argData);

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