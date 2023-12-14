using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AoC2023.Days.Day13
{
    internal class Day13Challenge : AoCChallengeBase
    {
        class Pattern
        {
            public int Id { get; set; }

            public bool[,]? Bitmap { get; set; }

            public int HorizontalReflectionLeftColumns { get; set; }
            public int VerticalReflectionTopRows { get; set; }
        }

        public override int Day => 13;
        public override string Name => "Point of Incidence";

        protected override object? ExpectedTestResultPartOne => 405;
        override protected object? ExpectedTestResultPartTwo => 400;

        private List<Pattern> _patterns = new();

        protected override object SolvePartOneInternal(string[] inputData)
        {
            this.ParseInput(inputData);
            foreach(var pattern in this._patterns)
            {
                this.CheckPatternReflection(pattern, false);
            }

            return this._patterns.Sum(p => p.HorizontalReflectionLeftColumns) 
                + 100 * this._patterns.Sum(p => p.VerticalReflectionTopRows);
        }

        protected override object SolvePartTwoInternal(string[] inputData)
        {
            this.ParseInput(inputData);
            foreach (var pattern in this._patterns)
            {
                this.CheckPatternReflection(pattern, true);
            }

            return this._patterns.Sum(p => p.HorizontalReflectionLeftColumns)
                + 100 * this._patterns.Sum(p => p.VerticalReflectionTopRows);
        }

        private void CheckPatternReflection(Pattern pattern, bool withSmudge)
        {
            var reflectionLine = this.FindReflectionLine(pattern.Bitmap!, null);

            if (withSmudge)
            {
                for(var x = 0; x < pattern.Bitmap!.GetLength(0); x++)
                {
                    for (var y = 0; y < pattern.Bitmap.GetLength(1); y++)
                    {
                        var bitmap = (bool[,])pattern.Bitmap!.Clone();
                        bitmap[x, y] = !bitmap[x, y];
                        var reflectionLine2 = this.FindReflectionLine(bitmap, reflectionLine);
                        if (reflectionLine2.hRefLeftCol > 0 || reflectionLine2.vRefTopRows > 0)
                        {
                            pattern.HorizontalReflectionLeftColumns = reflectionLine2.hRefLeftCol;
                            pattern.VerticalReflectionTopRows = reflectionLine2.vRefTopRows;
                            return;
                        }
                    }
                }
            } 
            else
            {
                pattern.HorizontalReflectionLeftColumns = reflectionLine.hRefLeftCol;
                pattern.VerticalReflectionTopRows = reflectionLine.vRefTopRows;
            }

            Debug.Assert(pattern.HorizontalReflectionLeftColumns > 0 || pattern.VerticalReflectionTopRows > 0);
        }

        private (int hRefLeftCol, int vRefTopRows) FindReflectionLine(bool[,] bitmap, (int hRefLeftCol, int vRefTopRows)? ignoreLine)
        {
            // check horizontal reflection
            for (var x = 1; x < bitmap.GetLength(0); x++)
            {
                var isReflection = true;
                for (var y = 0; y < bitmap.GetLength(1); y++)
                {
                    var reflectionLength = Math.Min(x, bitmap.GetLength(0) - x);
                    for (var dx = 0; dx < reflectionLength; dx++)
                    {
                        if (bitmap[(x - 1) - dx, y] != bitmap[x + dx, y])
                        {
                            isReflection = false;
                            break;
                        }
                    }
                }
                if (isReflection && (ignoreLine == null || x != ignoreLine.Value.hRefLeftCol))
                {
                    return (x, 0);
                }
            }

            // check vertical reflection
            for (var y = 1; y < bitmap.GetLength(1); y++)
            {
                var isReflection = true;
                var reflectionLength = Math.Min(y, bitmap.GetLength(1) - y);
                for (var dy = 0; dy < reflectionLength; dy++)
                {
                    for (var x = 0; x < bitmap.GetLength(0); x++)
                    {
                        if (bitmap[x, (y - 1) - dy] != bitmap[x, y + dy])
                        {
                            isReflection = false;
                            break;
                        }
                    }
                }

                if (isReflection && (ignoreLine == null || y != ignoreLine.Value.vRefTopRows))
                {
                    return (0, y);
                }
            }
            return (0, 0);
        }

        private void ParseInput(string[] inputData)
        {
            this._patterns.Clear();

            var patternId = 0;
            var pattern = new Pattern { Id = patternId++ };
            var patternStartLine = 0;

            for(var l=0; l<inputData.Length; l++)
            {
                if (string.IsNullOrEmpty(inputData[l]))
                {
                    this._patterns.Add(pattern);
                    pattern = new Pattern { Id = patternId++ };
                    patternStartLine = l + 1;
                    continue;
                }
                if(pattern.Bitmap == null)
                {
                    var patternHeight = 0;
                    for(var l2=l; l2<inputData.Length; l2++, patternHeight++)
                    {
                        if (string.IsNullOrEmpty(inputData[l2]))
                        {
                            break;
                        }
                    }
                    pattern.Bitmap = new bool[inputData[l].Length, patternHeight];
                }

                for (int i = 0; i < inputData[l].Length; i++)
                {
                    pattern.Bitmap![i, l - patternStartLine] = inputData[l][i] == '#';
                }
            }

            this._patterns.Add(pattern);
        }
    }
}
