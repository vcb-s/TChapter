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
using System.Linq;
using System.Text;
using System.Xml;
using TChapter.Chapters;

namespace TChapter.Util
{
    static class ConvertUtil
    {
        public static StringBuilder ToOGM(this MultiChapterData data, int index = 0, bool autoGenName = false)
        {
            var self = data[index];
            var lines = new StringBuilder();
            var name = ChapterName.GetChapterName();
            foreach (var item in self.Chapters.Where(c => c.Time != TimeSpan.MinValue))
            {
                lines.AppendLine($"CHAPTER{item.Number:D2}={item.Time2String(self.Expr)}");
                lines.AppendLine($"CHAPTER{item.Number:D2}NAME={(autoGenName ? name() : item.Name)}");
            }
            return lines;
        }

        public static string[] ToQPFILE(this MultiChapterData data, int index = 0)
        {
            var self = data[index];
            return self.Chapters.Where(c => c.Time != TimeSpan.MinValue)
                .Select(c => c.FramesInfo.ToString().Replace("*", "I").Replace("K", "I")).ToArray();
        }

        public static string[] ToCELLTIMES(this MultiChapterData data, int index = 0)
        {
            var self = data[index];
            return self.Chapters.Where(c => c.Time != TimeSpan.MinValue)
                .Select(c => ((long) Math.Round((decimal) c.Time.TotalSeconds * self.FramesPerSecond)).ToString()).ToArray();
        }

        public static string[] ToTIMECODES(this MultiChapterData data, int index = 0)
        {
            var self = data[index];
            return self.Chapters.Where(c => c.Time != TimeSpan.MinValue).Select(item => item.Time2String(self.Expr))
                .ToArray();
        }

        public static StringBuilder ToXML(this MultiChapterData data, string lang, int index = 0, bool autoGenName = false)
        {
            var self = data[index];
            if (string.IsNullOrWhiteSpace(lang)) lang = "und";
            var rndb = new Random();
            var xml = new StringBuilder();

            var xmlchap = XmlWriter.Create(xml, new XmlWriterSettings {Indent = true});
            xmlchap.WriteStartDocument();
            xmlchap.WriteComment("<!DOCTYPE Tags SYSTEM \"matroskatags.dtd\">");
            xmlchap.WriteStartElement("Chapters");
            xmlchap.WriteStartElement("EditionEntry");
            xmlchap.WriteElementString("EditionFlagHidden", "0");
            xmlchap.WriteElementString("EditionFlagDefault", "0");
            xmlchap.WriteElementString("EditionUID", Convert.ToString(rndb.Next(1, int.MaxValue)));
            var name = ChapterName.GetChapterName();
            foreach (var item in self.Chapters.Where(c => c.Time != TimeSpan.MinValue))
            {
                xmlchap.WriteStartElement("ChapterAtom");
                xmlchap.WriteStartElement("ChapterDisplay");
                xmlchap.WriteElementString("ChapterString", autoGenName ? name() : item.Name);
                xmlchap.WriteElementString("ChapterLanguage", lang);
                xmlchap.WriteEndElement();
                xmlchap.WriteElementString("ChapterUID", Convert.ToString(rndb.Next(1, int.MaxValue)));
                xmlchap.WriteElementString("ChapterTimeStart", item.Time2String(self.Expr) + "000");
                xmlchap.WriteElementString("ChapterFlagHidden", "0");
                xmlchap.WriteElementString("ChapterFlagEnabled", "1");
                xmlchap.WriteEndElement();
            }
            xmlchap.WriteEndElement();
            xmlchap.WriteEndElement();
            xmlchap.Flush();
            xmlchap.Close();

            return xml;
        }

        public static StringBuilder ToCUE(this MultiChapterData data, string sourceFileName, int index = 0, bool autoGenName = false)
        {
            var self = data[index];
            var cueBuilder = new StringBuilder();
            cueBuilder.AppendLine("REM Generate By ChapterTool");
            cueBuilder.AppendLine($"TITLE \"{self.Title}\"");

            cueBuilder.AppendLine($"FILE \"{sourceFileName}\" WAVE");
            var i = 0;
            var name = ChapterName.GetChapterName();
            foreach (var chapter in self.Chapters.Where(c => c.Time != TimeSpan.MinValue))
            {
                cueBuilder.AppendLine($"  TRACK {++i:D2} AUDIO");
                cueBuilder.AppendLine($"    TITLE \"{(autoGenName ? name() : chapter.Name)}\"");
                cueBuilder.AppendLine($"    INDEX 01 {chapter.Time.ToCueTimeStamp()}");
            }
            return cueBuilder;
        }

        public static StringBuilder ToJSON(this MultiChapterData data, int index = 0, bool autoGenName = false)
        {
            var self = data[index];
            var jsonBuilder = new StringBuilder();
            jsonBuilder.Append("{");
            jsonBuilder.Append("\"sourceName\":");
            jsonBuilder.Append(data.ChapterType == ChapterTypeEnum.MPLS ? $"\"{self.SourceName}.m2ts\"," : "\"undefined\",");
            jsonBuilder.Append("\"chapter\":");
            jsonBuilder.Append("[[");

            var baseTime = TimeSpan.Zero;
            Chapter prevChapter = null;
            var name = ChapterName.GetChapterName();
            foreach (var chapter in self.Chapters)
            {
                if (chapter.Time == TimeSpan.MinValue && prevChapter != null)
                {
                    baseTime = prevChapter.Time;//update base time
                    name = ChapterName.GetChapterName();
                    var initChapterName = autoGenName ? name() : prevChapter.Name;
                    jsonBuilder.Remove(jsonBuilder.Length - 1, 1);
                    jsonBuilder.Append("],[");
                    jsonBuilder.Append($"{{\"name\":\"{initChapterName}\",\"time\":0}},");
                    continue;
                }
                var time = chapter.Time - baseTime;
                var chapterName = (autoGenName ? name() : chapter.Name);
                jsonBuilder.Append($"{{\"name\":\"{chapterName}\",\"time\":{time.TotalSeconds}}},");
                prevChapter = chapter;
            }
            jsonBuilder.Remove(jsonBuilder.Length - 1, 1);
            jsonBuilder.Append("]]");
            jsonBuilder.Append("}");
            return jsonBuilder;
        }
    }
}
