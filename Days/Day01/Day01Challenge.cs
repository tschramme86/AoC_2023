using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AoC2023.Days.Day01
{
    internal class Day01Challenge : AoCChallengeBase
    {
        public override int Day => 1;
        public override string Name => "Trebuchet?!";

        protected override object ExpectedTestResultPartOne => 142;
        protected override object ExpectedTestResultPartTwo => 281;
        protected override bool ExtraTestDataPartTwo => true;

        protected override object SolvePartOneInternal(string[] inputData)
        {
            var sum = 0;
            foreach(var line in inputData)
            {
                sum += this.SumForLine(line);
            }
            return sum;
        }

        protected override object SolvePartTwoInternal(string[] inputData)
        {
            var sum = 0;
            var replaceWordByDigit = new Dictionary<string, int>
            {
                { "one", 1 }, { "1", 1 },
                { "two", 2 }, { "2", 2 },
                { "three", 3 }, { "3", 3 },
                { "four", 4 }, { "4", 4 },
                { "five", 5 }, { "5", 5 },
                { "six", 6 }, { "6", 6 },
                { "seven", 7 }, { "7", 7 },
                { "eight", 8 }, { "8", 8 },
                { "nine", 9 }, { "9", 9 },
            };
            foreach(var line in inputData)
            {
                var newLine = line;

                var firstDigit = 0;
                var match = System.Text.RegularExpressions.Regex.Match(newLine, @"one|1|two|2|three|3|four|4|five|5|six|6|seven|7|eight|8|nine|9");
                if(match.Success)
                {
                    firstDigit = replaceWordByDigit[match.Value];
                }
                match = System.Text.RegularExpressions.Regex.Match(newLine, @"one|1|two|2|three|3|four|4|five|5|six|6|seven|7|eight|8|nine|9", System.Text.RegularExpressions.RegexOptions.RightToLeft);
                var lastDigit = 0;
                if(match.Success)
                {
                    lastDigit = replaceWordByDigit[match.Value];
                }

                var number = firstDigit * 10 + lastDigit;
                sum += number;
            }
            return sum;
        }

        private int SumForLine(string line)
        {
            var firstDigit = 0;
            var lastDigit = 0;
            foreach (var c in line)
            {
                // c is digit?
                if (c >= '0' && c <= '9')
                {
                    firstDigit = c - '0';
                    break;
                }
            }
            foreach (var c in line.Reverse())
            {
                // c is digit?
                if (c >= '0' && c <= '9')
                {
                    lastDigit = c - '0';
                    break;
                }
            }
            var number = firstDigit * 10 + lastDigit;
            return number;
        }
    }
}
