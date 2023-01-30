// SPDX-License-Identifier: GPL-3.0-or-later
// SPDX-FileCopyrightText: Copyright 2017-2023 TautCony (i@tautcony.xyz)

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using FluentAssertions;
using TChapter.Object;

namespace TChapter.Test.Object
{
    [TestClass]
    public class ExpressionTests
    {
        private static void EvalAreEqual(decimal expected, string actual)
        {
            new Expression(actual).Eval().Should().Be(expected);
        }
        private static void EvalAreNearly(decimal expected, string actual)
        {
            new Expression(actual).Eval().Should().BeInRange(expected - 1e-10M, expected + 1e-10M);
        }

        [TestMethod]
        public void ExpressionConvertTest()
        {
            new Expression("2^10%10   + 6 \t///comment sample").ToString().Should().Be("2 10 ^ 10 % 6 +");
            new Expression("((a+b)*(c+d))/(((e)))").ToString().Should().Be("a b + c d + * e /");
        }

        [TestMethod]
        public void ExpressionPostFixTest()
        {
            var ret = new Expression("a b + c d + * e /".Split());
            var exp = new Expression("((a+b)*(c+d))/(((e)))");
            "((a + b) * (c + d)) / e".Should().Be(Expression.Postfix2Infix(ret.ToString()));
            exp.ToString().Should().Be(ret.ToString());
        }

        [TestMethod]
        public void ExpressionWEvalTest()
        {
            EvalAreEqual(2.9289682539682539682539682539682539M, "1+1/2+1/3+1/4+1/5+1/6+1/7+1/8+1/9+1/10");
            EvalAreEqual(0.3330078125M, "1-1/2-1/4+1/8-1/16+1/32-1/64+1/128-1/256+1/512-1/1024");
            EvalAreEqual(0.3330078125M, "1-(1/2)-(1/4)+(1/8)-(1/16)+(1/32)-(1/64)+(1/128)-(1/256)+(1/512)-(1/1024)");
            EvalAreEqual(256, "2^2^2^2");
            EvalAreEqual(65536, "2^(2^(2^2))");
        }

        [TestMethod]
        public void EmptyExpressionTest()
        {
            var ret = new Expression(string.Empty);
            ret.ToString().Should().BeEmpty();
            ret.Eval().Should().Be(0M);
        }

        [TestMethod]
        public void ExpressionWithFunctionTest()
        {
            var ret = new Expression("floor(1.133) + floor(log10(1023)) - ceil(0.9)");
            ret.ToString().Should().Be("1.133 floor 1023 log10 floor + 0.9 ceil -");
            ret.Eval().Should().Be(3M);
        }

        [TestMethod]
        public void FunctionAbsTest()
        {
            EvalAreEqual(1908.8976M, "abs(-1908.8976)");
            EvalAreEqual(1908.8976M, "abs(1908.8976)");

            EvalAreEqual(1908, "abs(-1908)");
            EvalAreEqual(1908, "abs(1908)");
        }

        [TestMethod]
        public void FunctionSctTest()
        {
            EvalAreNearly(1M, "sin(asin(1))");
            EvalAreNearly(1M, "cos(acos(1))");
            EvalAreNearly(1M, "tan(atan(1))");
        }

        [TestMethod]
        public void FunctionLog10Test()
        {
            EvalAreEqual(3.0M, "log10(1000.0)");
            EvalAreEqual(3.0M, "log10(1000.0)");
            EvalAreEqual(14.0M, "log10(10 ^ 14)");
            EvalAreEqual(3.73895612695404M, "log10(5482.2158)");
            EvalAreEqual(14.6615511428938M, "log10(458723662312872.125782332587)");
            EvalAreEqual(-0.908382862219234M, "log10(0.12348583358871)");
        }

        [TestMethod]
        public void Uva12803Test()
        {
            var path = Path.Combine(Configuration.TestCaseBasePath, @"UVA12803");

            var timer = new System.Diagnostics.Stopwatch();
            timer.Start();
            var input  = File.ReadAllLines(Path.Combine(path, "expression.in"));
            var output = File.ReadAllLines(Path.Combine(path, "expression.out"));
            for (var i = 0; i < input.Length; ++i)
            {
                var tmp = new Expression(input[i]).Eval().ToString("0.00");
                tmp.Should().Be(output[i]);
                Console.Write($"{tmp} ");
            }
            timer.Stop();
            Console.WriteLine($"\nDuration: {timer.Elapsed.Milliseconds}ms");
        }

        [TestMethod]
        public void Postfix2InfixTest()
        {
            Expression.Postfix2Infix("x y 64 * + z 256 * + 3 /").Should().Be("((x + (y * 64)) + (z * 256)) / 3");
            Expression.Postfix2Infix("x y + z + 3 /").Should().Be("((x + y) + z) / 3");
        }

        /*
        [TestMethod]
        public void TernaryOperatorTest()
        {
            //x>22 ? x<96 ? 4*(x-16)^2+40000 : 65536:0
            //x 22 > x 96 < x 16 - dup * 4 * 40000 + 65536 ? 0 ?

            //a 20 < 65535 a 40 < x a 80 < y z ? ? ?
            //a<20 ? 65535 : a < 40? x : a < 80 ? y : z

            //x 32768 - -512 > 32768 32768 32768 x - 256 / sqrt 4 min 256 * - ?
            //32768-x < 512 ? 32768 : 32768 - 256*min{sqrt[|32768-x|/256],4}
        }
        */
    }
}
