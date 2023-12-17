using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AoC2023.Days.Day15
{
    internal class Day15Challenge : AoCChallengeBase
    {
        public override int Day => 15;
        public override string Name => "Lens Library";

        protected override object? ExpectedTestResultPartOne => 1320;

        protected override object? ExpectedTestResultPartTwo => 145;

        override protected object SolvePartOneInternal(string[] inputData)
        {
            var strings = inputData[0].Split(',');
            var sum = 0;
            foreach(var s in strings)
            {
                sum += this.HashData(s);
            }

            return sum;
        }

        private static readonly Regex rxLensOp = new Regex(@"^(?<LensName>[a-z]{2,10})((?<OpMinus>\-)|(?<OpEq>=(?<OpEqV>\d+)))$", RegexOptions.Compiled);

        override protected object SolvePartTwoInternal(string[] inputData)
        {
            var strings = inputData[0].Split(',');
            var boxes = new List<(string label, int focalLength)>[256];
            for(int i=0; i<boxes.Length; i++) boxes[i] = new List<(string label, int focalLength)>();

            foreach (var s in strings)
            {
                var match = rxLensOp.Match(s);
                if (match.Success)
                {
                    var lensName = match.Groups["LensName"].Value;
                    var opMinus = match.Groups["OpMinus"].Success;
                    var opEq = match.Groups["OpEq"].Success;
                    var opEqV = opEq ? int.Parse(match.Groups["OpEqV"].Value) : 0;
                    var boxId = this.HashData(lensName);
                    if (opMinus)
                    {
                        var idx = boxes[boxId].FindIndex(b => b.label == lensName);
                        if (idx >= 0)
                        {
                            boxes[boxId].RemoveAt(idx);
                        }
                    }
                    else
                    {
                        var idx = boxes[boxId].FindIndex(b => b.label == lensName);
                        if (idx >= 0)
                        {
                            boxes[boxId][idx] = (lensName, opEqV);
                        } else {
                            boxes[boxId].Add((lensName, opEqV));
                        }
                    }
                } else
                {
                    Console.WriteLine($"Invalid lens operation {s}");
                }
            }

            var focusPower = 0;
            for(var i=0; i<boxes.Length; i++)
            {
                var box = boxes[i];
                if (box.Count == 0) continue;

                for(var lIdx = 0; lIdx < box.Count; lIdx++)
                {
                    focusPower += (i + 1) * (lIdx + 1) * box[lIdx].focalLength;
                }
            }

            return focusPower;
        }

        private int HashData(string data)
        {
            var currentValue = 0;
            foreach (var c in data)
            {
                currentValue += c;
                currentValue *= 17;
                currentValue %= 256;
            }
            return currentValue;
        }
    }
}
