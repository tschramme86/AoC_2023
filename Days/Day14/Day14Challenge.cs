using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AoC2023.Days.Day14
{
    internal class Day14Challenge : AoCChallengeBase
    {
        class RockCycleWrapper
        {
            private bool[,] _bitmap;

            public RockCycleWrapper(bool[,] bitmap)
            {

                _bitmap = (bool[,])bitmap.Clone();
            }

            public override bool Equals(object? obj)
            {
                if (obj is RockCycleWrapper other)
                {
                    for (var y = 0; y < this._bitmap!.GetLength(1); y++)
                    {
                        for (var x = 0; x < this._bitmap.GetLength(0); x++)
                        {
                            if (this._bitmap[x, y] != other._bitmap[x, y])
                                return false;
                        }
                    }
                    return true;
                }
                return false;
            }

            override public int GetHashCode()
            {
                var hash = new HashCode();
                for (var y = 0; y < this._bitmap!.GetLength(1); y++)
                {
                    for (var x = 0; x < this._bitmap.GetLength(0); x++)
                    {
                        hash.Add(this._bitmap[x, y]);
                    }
                }
                return hash.ToHashCode();
            }
        }

        enum Direction
        {
            North,
            East,
            South,
            West
        }

        override public int Day => 14;
        override public string Name => "Parabolic Reflector Dish";

        protected override object? ExpectedTestResultPartOne => 136;
        override protected object? ExpectedTestResultPartTwo => 64;

        private bool[,]? _bitmap;
        private bool[,]? _roundRocks;

        private Dictionary<RockCycleWrapper, int> _cycleDict = new();

        protected override object SolvePartOneInternal(string[] inputData)
        {
            this.ParseInputData(inputData);
            this.Tilt(Direction.North);
            return this.LoadNorth();
        }

        override protected object SolvePartTwoInternal(string[] inputData)
        {
            this.ParseInputData(inputData);
            var cycles = 1000000000;
            var cycleIdx = 0;
            var cycleFound = false;
            var useCycleDetection = true;
            do
            {
                this.Tilt(Direction.North);
                this.Tilt(Direction.West);
                this.Tilt(Direction.South);
                this.Tilt(Direction.East);

                if (!cycleFound && useCycleDetection)
                {
                    var cycleWrapper = new RockCycleWrapper(this._roundRocks!);
                    if (this._cycleDict.TryGetValue(cycleWrapper, out var cycle))
                    {
                        var cycleLength = cycleIdx - cycle;
                        var remainingCycles = (cycles - cycleIdx) - 1;
                        var remainingCyclesModulo = remainingCycles % cycleLength;
                        cycleIdx = cycles - remainingCyclesModulo;
                        cycleFound = true;
                    }
                    else
                    {
                        this._cycleDict.Add(cycleWrapper, cycleIdx);
                    }
                }
            } while (cycleIdx++ < cycles);
            return this.LoadNorth();
        }

        private void Tilt(Direction direction)
        {
            bool couldMove;
            do
            {
                couldMove = false;
                if (direction == Direction.North)
                {
                    for (var y = 1; y < this._bitmap!.GetLength(1); y++)
                    {
                        for (var x = 0; x < this._bitmap.GetLength(0); x++)
                        {
                            if (this._roundRocks![x, y] && !this._roundRocks[x, y - 1] && !this._bitmap[x, y - 1])
                            {
                                this._roundRocks[x, y - 1] = true;
                                this._roundRocks[x, y] = false;
                                couldMove = true;
                            }
                        }
                    }
                } 
                else if(direction == Direction.South)
                {
                    for (var y = this._bitmap!.GetLength(1) - 2; y >= 0; y--)
                    {
                        for (var x = 0; x < this._bitmap.GetLength(0); x++)
                        {
                            if (this._roundRocks![x, y] && !this._roundRocks[x, y + 1] && !this._bitmap[x, y + 1])
                            {
                                this._roundRocks[x, y + 1] = true;
                                this._roundRocks[x, y] = false;
                                couldMove = true;
                            }
                        }
                    }
                }
                else if(direction == Direction.West)
                {
                    for (var x = 1; x < this._bitmap!.GetLength(0); x++)
                    {
                        for (var y = 0; y < this._bitmap.GetLength(1); y++)
                        {
                            if (this._roundRocks![x, y] && !this._roundRocks[x - 1, y] && !this._bitmap[x - 1, y])
                            {
                                this._roundRocks[x - 1, y] = true;
                                this._roundRocks[x, y] = false;
                                couldMove = true;
                            }
                        }
                    }
                }
                else if(direction == Direction.East)
                {
                    for(var x = this._bitmap!.GetLength(0) - 2; x >= 0; x--)
                    {
                        for (var y = 0; y < this._bitmap.GetLength(1); y++)
                        {
                            if (this._roundRocks![x, y] && !this._roundRocks[x + 1, y] && !this._bitmap[x + 1, y])
                            {
                                this._roundRocks[x + 1, y] = true;
                                this._roundRocks[x, y] = false;
                                couldMove = true;
                            }
                        }
                    }
                }
            } while(couldMove);
        }

        private int LoadNorth()
        {
            /*
            if(this.IsOnTestData)
                this.PrintPattern();
            */

            var totalLoad = 0;
            var south = this._roundRocks!.GetLength(1) - 1;
            for (var y = 0; y < this._roundRocks!.GetLength(1); y++)
            {
                for (var x = 0; x < this._roundRocks.GetLength(0); x++)
                {
                    if (this._roundRocks![x, y])
                    {
                        totalLoad += (south - y) + 1;
                    }
                }
            }
            return totalLoad;
        }

        private void ParseInputData(string[] inputData)
        {
            this._bitmap = new bool[inputData[0].Length, inputData.Length];
            this._roundRocks = new bool[inputData[0].Length, inputData.Length];
            for (var y = 0; y < inputData.Length; y++)
            {
                for (var x = 0; x < inputData[y].Length; x++)
                {
                    this._bitmap[x, y] = inputData[y][x] == '#';
                    this._roundRocks[x, y] = inputData[y][x] == 'O';
                }
            }
        }

        private void PrintPattern()
        {
            for(var y=0; y < this._bitmap!.GetLength(1); y++)
            {
                for(var x=0; x < this._bitmap.GetLength(0); x++)
                {
                    Console.Write(this._bitmap[x, y] ? '#' : (this._roundRocks![x, y] ? 'O' : '.'));
                }
                Console.WriteLine();
            }
        }
    }
}
