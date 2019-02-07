using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TChapter.Parsing;

namespace TChapter.Test.Parsing
{
    [TestClass]
    public class IFOTimeTest
    {
        [TestMethod]
        public void TestMethod1()
        {
            var times = new[]
            {
                new[] {0, 0,  5,  0}, new[] {0, 0, 15,  0},
                new[] {0, 1, 29, 28}, new[] {0, 0, 10,  0},
                new[] {0, 7, 54, 16}, new[] {0, 6, 40, 16},
                new[] {0, 5,  8, 22}, new[] {0, 1, 19, 28},
                new[] {0, 0, 14, 28}, new[] {0, 0, 10,  2},
                new[] {0, 0,  6,  0}, new[] {0, 0,  5,  0},
                new[] {0, 2, 44, 26}, new[] {0, 1, 29, 26},
                new[] {0, 0, 10,  0}, new[] {0, 5, 35, 20},
                new[] {0, 5, 21, 20}, new[] {0, 6, 16, 18},
                new[] {0, 1, 19, 28}, new[] {0, 0, 14, 28},
                new[] {0, 0, 10,  0}, new[] {0, 0,  6,  0}
            }.Select(time => new IfoTimeSpan(time[0], time[1], time[2], time[3], true));
            var exceptedFrames = new[]
            {
                150,   600,  3298,  3598, 17834,
                29850, 39112, 41510, 41958, 42260,
                42440, 42590, 47536, 50232, 50532,
                60602, 70252, 81550, 83948, 84396,
                84696, 84876
            };
            var total = new IfoTimeSpan(true);
            var frames = new List<long>();
            foreach (var time in times)
            {
                total += time;
                frames.Add(total.TotalFrames);
                var tsp = (TimeSpan)total;
                Console.WriteLine($"{tsp.Hours:D2}:{tsp.Minutes:D2}:{tsp.Seconds:D2}.{tsp.Milliseconds:D3} {total.TotalFrames}");
            }
            frames.Should().BeEquivalentTo(exceptedFrames);
            ((int)Math.Round((decimal)((TimeSpan)total).TotalSeconds * (30000M / 1001))).Should().Be(84876);
        }
    }
}

