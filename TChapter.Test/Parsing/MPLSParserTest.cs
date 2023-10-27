// SPDX-License-Identifier: GPL-3.0-or-later
// SPDX-FileCopyrightText: Copyright 2017-2023 TautCony (i@tautcony.xyz)

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TChapter.Chapters;
using TChapter.Parsing;

namespace TChapter.Test.Parsing
{
    [TestClass]
    public class MPLSParserTest
    {
        [TestMethod]
        public void TestParseMPLS_HD()
        {
            IChapterParser parser = new MPLSParser();
            var data = parser.Parse(Path.Combine(Configuration.TestCaseBasePath, "MPLS", "00001-HD.mpls"));

            var text = new StringBuilder();
            var ts = new List<long>();

            foreach (var chapter in data)
            {
                chapter.UpdateFrameInfo();
                foreach (var item in chapter.Chapters)
                {
                    text.AppendLine(item + " " + item.FramesInfo);
                    ts.Add(item.Time.Ticks);
                }
                text.AppendLine();
            }

            Console.WriteLine(text);
            var expected = new[]
            {
                0, 749920000, 1650400000, 7470380000, 13160230000, 14060300000, 10010000, 619790000, 1520270000, 6910240000,
                13159810000, 14059880000
            };
            ts.Should().BeEquivalentTo(expected);
        }

        [TestMethod]
        public void TestParseMPLS_UHD()
        {
            IChapterParser parser = new MPLSParser();
            var data = parser.Parse(Path.Combine(Configuration.TestCaseBasePath, "MPLS", "00001-UHD.mpls"));
            
            var text = new StringBuilder();
            var ts = new List<long>();

            foreach (var chapter in data)
            {
                chapter.UpdateFrameInfo();
                foreach (var item in chapter.Chapters)
                {
                    text.AppendLine(item + " " + item.FramesInfo);
                    ts.Add(item.Time.Ticks);
                }
                text.AppendLine();
            }

            Console.WriteLine(text);
            var expected = new[]
            {
                0, 3001330000, 6727550000, 10940930000, 13985220000, 16745900000, 20654800000, 24693420000, 27704340000,
                30590980000, 34078630000, 37538750000, 40267730000, 44501960000, 48658190000, 53045910000
            };
            ts.Should().BeEquivalentTo(expected);
        }
        
        [TestMethod]
        public void TestParseMPLS_Empty()
        {
            IChapterParser parser = new MPLSParser();
            var data = parser.Parse(Path.Combine(Configuration.TestCaseBasePath, "MPLS", "00001-Empty.mpls"));
            
            var text = new StringBuilder();
            var ts = new List<long>();

            foreach (var chapter in data)
            {
                chapter.UpdateFrameInfo();
                foreach (var item in chapter.Chapters)
                {
                    text.AppendLine(item + " " + item.FramesInfo);
                    ts.Add(item.Time.Ticks);
                }
                text.AppendLine();
            }

            Console.WriteLine(text);
            var expected = new[]
            {
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
            };
            ts.Should().BeEquivalentTo(expected);
        }
    }
}
