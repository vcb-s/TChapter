// SPDX-License-Identifier: GPL-3.0-or-later
// SPDX-FileCopyrightText: Copyright 2017-2023 TautCony (i@tautcony.xyz)

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
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
            var jsonObject = new Dictionary<string, dynamic>();
            var chapters = new List<dynamic>();
            jsonObject.Add("sourceName", data.ChapterType == ChapterTypeEnum.MPLS ? $"{self.SourceName}.m2ts" : "undefined");
            jsonObject.Add("chapter", chapters);

            var baseTime = TimeSpan.Zero;
            Chapter prevChapter = null;
            var name = ChapterName.GetChapterName();
            foreach (var chapter in self.Chapters)
            {
                if (chapter.Time == TimeSpan.MinValue && prevChapter != null)
                {
                    baseTime = prevChapter.Time; // update base time
                    name = ChapterName.GetChapterName();
                    var initChapterName = autoGenName ? name() : prevChapter.Name;
                    chapters.Add(new Dictionary<string, dynamic>
                    {
                        { "name", initChapterName },
                        { "time", 0 }
                    });
                    continue;
                }
                var time = chapter.Time - baseTime;
                var chapterName = (autoGenName ? name() : chapter.Name);

                chapters.Add(new Dictionary<string, dynamic>
                {
                     { "name", chapterName },
                     { "time", time.TotalSeconds }
                });
                prevChapter = chapter;
            }
            return new StringBuilder(Serialize(jsonObject));
        }

        private static string Serialize<T>(T aObject) where T : new()
        {
            var ms = new MemoryStream();
            var ser = new DataContractJsonSerializer(typeof(T));
            ser.WriteObject(ms, aObject);
            var json = ms.ToArray();
            ms.Close();
            return Encoding.UTF8.GetString(json, 0, json.Length);
        }
    }
}
