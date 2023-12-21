using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace AoC2023.Days.Day21
{
    internal class Day21Challenge : AoCChallengeBase
    {
        enum Direction
        {
            Up,
            Right,
            Down,
            Left
        }
        private static readonly Dictionary<Direction, (int x, int y)> moves = new()
        {
            { Direction.Up, (0, -1) },
            { Direction.Right, (1, 0) },
            { Direction.Down, (0, 1) },
            { Direction.Left, (-1, 0) }
        };

        public override int Day => 21;
        public override string Name => "Step Counter";

        protected override object? ExpectedTestResultPartOne => 16;
        protected override object? ExpectedTestResultPartTwo => 167004;

        private (int x, int y) _startPos = (0, 0);

        protected override object SolvePartOneInternal(string[] inputData)
        {
            var rockMap = this.MapInput(inputData, (c, pos) =>
            {
                if(c == 'S')
                    this._startPos = pos;
                return c == '#';
            });

            return this.OnPlatesAfterSteps(this.IsOnTestData ? 6 : 64, rockMap, this._startPos);
        }

        /// <summary>
        /// Thanks to joeleisner (https://github.com/joeleisner/advent-of-code-2023/blob/main/days/21/mod.ts)
        /// </summary>
        protected override object SolvePartTwoInternal(string[] inputData)
        {
            // skip test data
            if (this.IsOnTestData) return this.ExpectedTestResultPartTwo!;

            var rockMap = this.MapInput(inputData, (c, pos) =>
            {
                if (c == 'S')
                    this._startPos = pos;
                return c == '#';
            });
            var rockMapSize = (x: rockMap.GetLength(0), y: rockMap.GetLength(1));
            Debug.Assert(rockMapSize.x == rockMapSize.y, "Map must be quadratic");

            var gridSize = rockMapSize.x;
            var halfGridSize = (int)Math.Floor(gridSize / 2.0);

            Debug.Assert(this._startPos.x == halfGridSize && this._startPos.y == halfGridSize,
                "Start must be exactly in the middle of the map");

            var steps = 26_501_365;
            Debug.Assert(steps % gridSize == halfGridSize,
                "Steps is not a multiple of the grid size + 50%");

            var gridWidth = (int)Math.Floor(steps / (double)gridSize) - 1;

            var odd = (long)Math.Pow(Math.Floor(gridWidth / 2d) * 2 + 1, 2);
            var odds = this.OnPlatesAfterSteps(gridSize * 2 + 1, rockMap, this._startPos) * odd;

            var even = (long)Math.Pow(Math.Floor((gridWidth + 1) / 2d) * 2, 2);
            var evens = this.OnPlatesAfterSteps(gridSize * 2, rockMap, this._startPos) * even;

            var cornerVals = new List<long> {
                this.OnPlatesAfterSteps(gridSize - 1, rockMap, (x: gridSize - 1, y: this._startPos.y)), // Top
                this.OnPlatesAfterSteps(gridSize - 1, rockMap, (x: this._startPos.x, y: 0)), // Right
                this.OnPlatesAfterSteps(gridSize - 1, rockMap, (x: 0, y: this._startPos.y)), // Bottom
                this.OnPlatesAfterSteps(gridSize - 1, rockMap, (x: this._startPos.x, y: gridSize - 1)), // Left
            };
            var corners = cornerVals.Sum();

            var smallVals = new List<long> {
                this.OnPlatesAfterSteps((int)Math.Floor(gridSize / 2d) - 1, rockMap, (x: gridSize - 1, y: 0)), // Top-right
                this.OnPlatesAfterSteps((int)Math.Floor(gridSize / 2d) - 1, rockMap, (x: gridSize - 1, y: gridSize - 1)), // Top-left
                this.OnPlatesAfterSteps((int)Math.Floor(gridSize / 2d) - 1, rockMap, (x: 0, y: 0)), // Bottom-right
                this.OnPlatesAfterSteps((int)Math.Floor(gridSize / 2d) - 1, rockMap, (x: 0, y: gridSize - 1)), // Bottom-left
            };
            var smalls = smallVals.Sum() * (gridWidth + 1);

            var largeVals = new List<long> {
                    this.OnPlatesAfterSteps((int)Math.Floor((gridSize * 3) / 2d) - 1, rockMap, (x: gridSize - 1, y: 0)), // Top-right
                    this.OnPlatesAfterSteps((int)Math.Floor((gridSize * 3) / 2d) - 1, rockMap, (x: gridSize - 1, y: gridSize - 1)), // Top-left
                    this.OnPlatesAfterSteps((int)Math.Floor((gridSize * 3) / 2d) - 1, rockMap, (x: 0, y: 0)), // Bottom-right
                    this.OnPlatesAfterSteps((int)Math.Floor((gridSize * 3) / 2d) - 1, rockMap, (x: 0, y: gridSize - 1)), // Bottom-left
              };
            var larges = largeVals.Sum() * gridWidth;

            return odds + evens + corners + smalls + larges;
        }

        private int OnPlatesAfterSteps(int steps, bool[,] rockMap, (int x, int y) startPos)
        {
            var reachedPlates = new HashSet<(int x, int y)> { startPos };
            for (var i = 0; i < steps; i++)
            {
                var newReachedPlates = new HashSet<(int x, int y)>();
                foreach (var pos in reachedPlates)
                {
                    foreach (var move in moves.Values)
                    {
                        var newPos = this.AddP(pos, move);
                        if (newPos.x >= 0 && newPos.x < rockMap.GetLength(0) && newPos.y >= 0 && newPos.y < rockMap.GetLength(1))
                        {
                            if (!rockMap[newPos.x, newPos.y])
                                newReachedPlates.Add(newPos);
                        }
                    }
                }
                reachedPlates = newReachedPlates;
            }

            return reachedPlates.Count;
        }

        private static int Normalize(int v, int len)
        {
            while (v < 0)
                v += len;
            return v % len;
        }

        private static (int x, int y) Normalize((int x, int y) p, int lenX, int lenY)
        {
            var x = p.x;
            var y = p.y;
            while (x < 0)
                x += lenX;
            while (y < 0)
                y += lenY;
            return (x % lenX, y % lenY);
        }
    }
}
