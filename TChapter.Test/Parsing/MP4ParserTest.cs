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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TChapter.Chapters;
using TChapter.Parsing;

namespace TChapter.Test.Parsing
{
    [TestClass]
    public class MP4ParserTest
    {
        [TestMethod]
        public void TestParseMP4_nero()
        {
            IChapterParser parser = new MP4Parser();
            var data = parser.Parse(@"..\..\..\Assert\MP4\nero.mp4");
            Console.WriteLine(data);
            foreach (var chapter in (data as SingleChapterData).Chapters)
            {
                Console.WriteLine(chapter);
            }
        }
    }
}
