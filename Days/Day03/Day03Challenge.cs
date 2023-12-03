using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AoC2023.Days.Day03
{
    internal class Day03Challenge :AoCChallengeBase
    {
        private static readonly Regex RxNumber = new Regex(@"\d+", RegexOptions.Compiled);

        override public int Day => 3;
        override public string Name => "Gear Ratios";
        protected override object? ExpectedTestResultPartOne => 4361;
        protected override object? ExpectedTestResultPartTwo => 467835L;

        protected override object SolvePartOneInternal(string[] inputData)
        {
            var sumOfParts = 0;

            for(var i=0; i<inputData.Length; i++)
            {
                var line = inputData[i];
                var match = RxNumber.Match(line);
                while(match.Success)
                {
                    var number = int.Parse(match.Value);

                    // search for symbol around number
                    for(var y = i - 1; y <= i + 1; y++)
                    {
                        if(y < 0 || y >= inputData.Length)
                        {
                            continue;
                        }
                        var line2 = inputData[y];
                        for(var x = match.Index - 1; x <= match.Index + match.Length; x++)
                        {
                            if(x < 0 || x >= line2.Length)
                            {
                                continue;
                            }
                            if (line2[x] != '.' && (line2[x] < '0' || line2[x] > '9'))
                            {
                                sumOfParts += number;
                            }
                        }
                    }

                    match = match.NextMatch();
                }
            }

            return sumOfParts;
        }

        protected override object SolvePartTwoInternal(string[] inputData)
        {
            var sumOfGearRatios = 0L;
            for (var i = 0; i < inputData.Length; i++)
            {
                var line = inputData[i];
                for(var x = 0; x < line.Length; x++)
                {
                    if (line[x] == '*')
                    {
                        // check if this is a gear, there must be two adjacent numbers
                        var numbers = new List<int>();
                        for(var y=i-1;y<=i+1;y++)
                        {
                            if(y < 0 || y >= inputData.Length)
                            {
                                continue;
                            }
                            var line2 = inputData[y];
                            var match = RxNumber.Match(line2);
                            while(match.Success)
                            {
                                if(match.Index - 1 <= x && x <= match.Index + match.Length)
                                {
                                    numbers.Add(int.Parse(match.Value));
                                }
                                match = match.NextMatch();
                            }
                        }
                        if(numbers.Count == 2)
                        {
                            sumOfGearRatios += numbers[0] * numbers[1];
                        }
                    }
                }
            }
            return sumOfGearRatios;
        }
    }
}
