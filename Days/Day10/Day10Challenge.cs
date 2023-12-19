using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AoC2023.Days.Day10
{
    internal class Day10Challenge : AoCChallengeBase
    {
        enum PipeType
        {
            NS,
            EW,
            NE, NW, 
            SE, SW
        }
        enum Direction
        {
            N, E, S, W
        }

        private Dictionary<Direction, (int x, int y)> _directions = new()
        {
            { Direction.N, (0, -1) },
            { Direction.E, (1, 0) },
            { Direction.S, (0, 1) },
            { Direction.W, (-1, 0) }
        };
        private Dictionary<Direction, Direction> _opposite = new()
        {
            { Direction.N, Direction.S },
            { Direction.E, Direction.W },
            { Direction.S, Direction.N },
            { Direction.W, Direction.E }
        };
        private Dictionary<(PipeType pipe, Direction comingFrom), Direction> _goingTo = new()
        {
            {(PipeType.NS, Direction.N), Direction.S },
            {(PipeType.NS, Direction.S), Direction.N },
            {(PipeType.EW, Direction.E), Direction.W },
            {(PipeType.EW, Direction.W), Direction.E },
            {(PipeType.NE, Direction.N), Direction.E },
            {(PipeType.NE, Direction.E), Direction.N },
            {(PipeType.NW, Direction.N), Direction.W },
            {(PipeType.NW, Direction.W), Direction.N },
            {(PipeType.SE, Direction.S), Direction.E },
            {(PipeType.SE, Direction.E), Direction.S },
            {(PipeType.SW, Direction.S), Direction.W },
            {(PipeType.SW, Direction.W), Direction.S }
        };

        public override int Day => 10;
        override public string Name => "Pipe Maze";

        protected override object? ExpectedTestResultPartOne => 4;
        protected override object? ExpectedTestResultPartTwo => 4;
        protected override bool ExtraTestDataPartTwo => true;

        private Dictionary<(int x, int y), PipeType> _pipes = new();
        private List<(int x, int y)> _groundTiles = new();
        private (int x, int y) _startPos = (0, 0);
        private List<PipeType> _possibleStartPipes = new();

        protected override object SolvePartOneInternal(string[] inputData)
        {
            this.ReadMaze(inputData);

            foreach(var pipe in this._possibleStartPipes)
            {
                var loop = this.FindLoop(this._startPos, pipe);
                if(loop != null)
                {
                    return loop.Count / 2;
                }
            }

            return 0;
        }

        protected override object SolvePartTwoInternal(string[] inputData)
        {
            this.ReadMaze(inputData);

            var width = inputData[0].Length;
            var height = inputData.Length;
            foreach (var pipe in this._possibleStartPipes)
            {
                var loop = this.FindLoop(this._startPos, pipe);
                if (loop != null)
                {
                    // remove clutter parts
                    foreach(var pos in this._pipes.Keys.ToList())
                    {
                        if(!loop.Contains(pos))
                        {
                            this._pipes.Remove(pos);
                            this._groundTiles.Add(pos);
                        }
                    }
                    this._pipes[_startPos] = pipe;

                    // create bitmap from loop
                    var bitmap = new bool[width * 3, height * 3];
                    foreach(var pos in loop)
                    {
                        switch (this._pipes[pos])
                        {
                            case PipeType.NS:
                                bitmap[pos.x * 3 + 1, pos.y * 3] = true;
                                bitmap[pos.x * 3 + 1, pos.y * 3 + 1] = true;
                                bitmap[pos.x * 3 + 1, pos.y * 3 + 2] = true;
                                break;
                            case PipeType.EW:
                                bitmap[pos.x * 3, pos.y * 3 + 1] = true;
                                bitmap[pos.x * 3 + 1, pos.y * 3 + 1] = true;
                                bitmap[pos.x * 3 + 2, pos.y * 3 + 1] = true;
                                break;
                            case PipeType.NE:
                                bitmap[pos.x * 3 + 1, pos.y * 3] = true;
                                bitmap[pos.x * 3 + 1, pos.y * 3 + 1] = true;
                                bitmap[pos.x * 3 + 2, pos.y * 3 + 1] = true;
                                break;
                            case PipeType.NW:
                                bitmap[pos.x * 3 + 1, pos.y * 3] = true;
                                bitmap[pos.x * 3 + 1, pos.y * 3 + 1] = true;
                                bitmap[pos.x * 3, pos.y * 3 + 1] = true;
                                break;
                            case PipeType.SE:
                                bitmap[pos.x * 3 + 1, pos.y * 3 + 1] = true;
                                bitmap[pos.x * 3 + 1, pos.y * 3 + 2] = true;
                                bitmap[pos.x * 3 + 2, pos.y * 3 + 1] = true;
                                break;
                            case PipeType.SW:
                                bitmap[pos.x * 3 + 1, pos.y * 3 + 1] = true;
                                bitmap[pos.x * 3 + 1, pos.y * 3 + 2] = true;
                                bitmap[pos.x * 3, pos.y * 3 + 1] = true;
                                break;
                        }
                    }

                    var tilesInside = 0;
                    foreach(var groudTile in this._groundTiles)
                    {
                        var positionsToCheck = new List<(int x, int y)> { (groudTile.x * 3 + 1, groudTile.y * 3 + 1) };
                        var positionsChecked = new HashSet<(int x, int y)>();
                        var foundOutside = false;
                        do
                        {
                            var newCheckPositions = new List<(int x, int y)>();
                            foreach(var pos in positionsToCheck)
                            {
                                if(pos.x < 0 || pos.x >= bitmap.GetLength(0) || pos.y < 0 || pos.y >= bitmap.GetLength(1))
                                {
                                    foundOutside = true;
                                    break;
                                }
                                if (!bitmap[pos.x, pos.y])
                                {
                                    this.CheckAndAdd((pos.x - 1, pos.y), newCheckPositions, positionsChecked);
                                    this.CheckAndAdd((pos.x + 1, pos.y), newCheckPositions, positionsChecked);
                                    this.CheckAndAdd((pos.x, pos.y - 1), newCheckPositions, positionsChecked);
                                    this.CheckAndAdd((pos.x, pos.y + 1), newCheckPositions, positionsChecked);
                                    this.CheckAndAdd((pos.x - 1, pos.y - 1), newCheckPositions, positionsChecked);
                                    this.CheckAndAdd((pos.x + 1, pos.y - 1), newCheckPositions, positionsChecked);
                                    this.CheckAndAdd((pos.x - 1, pos.y + 1), newCheckPositions, positionsChecked);
                                    this.CheckAndAdd((pos.x + 1, pos.y + 1), newCheckPositions, positionsChecked);
                                }
                            }
                            if (foundOutside) break;
                            positionsToCheck = newCheckPositions;
                        } while (positionsToCheck.Count > 0);

                        if(!foundOutside) tilesInside++;
                    }

                    return tilesInside;
                }
            }

            return 0;
        }

        private void CheckAndAdd((int x, int y) pos, List<(int x, int y)> positionsToCheck, HashSet<(int x, int y)> positionsChecked)
        {
            if (!positionsChecked.Contains(pos))
            {
                positionsToCheck.Add(pos);
                positionsChecked.Add(pos);
            }
        }

        private List<(int x, int y)>? FindLoop((int x, int y) startPos, PipeType startPipe)
        {
            var currentPos = startPos;
            var currentPipe = startPipe;
            var nextDirection = currentPipe switch
            {
                PipeType.NS => Direction.S,
                PipeType.EW => Direction.W,
                PipeType.NE => Direction.E,
                PipeType.NW => Direction.W,
                PipeType.SE => Direction.E,
                PipeType.SW => Direction.W,
                _ => throw new Exception("Unknown pipe type")
            };

            var loop = new List<(int x, int y)>();
            var loopFound = false;
            do
            {
                var nextPos = this.AddP(currentPos, this._directions[nextDirection]);
                if (nextPos == startPos)
                {
                    loop.Add(nextPos);
                    loopFound = true;
                    break;
                }

                if (nextDirection == Direction.N && !this.SouthConnected(nextPos))
                {
                    break;
                }
                if (nextDirection == Direction.E && !this.WestConnected(nextPos))
                {
                    break;
                }
                if (nextDirection == Direction.S && !this.NorthConnected(nextPos))
                {
                    break;
                }
                if (nextDirection == Direction.W && !this.EastConnected(nextPos))
                {
                    break;
                }
                currentPos = nextPos;
                currentPipe = this._pipes[currentPos];
                nextDirection = this._goingTo[(currentPipe, this._opposite[nextDirection])];
                loop.Add(currentPos);
            } while (true);

            if(loopFound)
                return loop;
            return null;
        }

        private void ReadMaze(string[] inputData)
        {
            this._pipes.Clear();
            this._groundTiles.Clear();

            (int x, int y) start = (0, 0);
            var startFound = false;
            for(var y=0; y<inputData.Length; y++)
            {
                for(var x=0; x < inputData[y].Length; x++)
                {
                    switch (inputData[y][x])
                    {
                        case '.':
                            this._groundTiles.Add((x, y));
                            break;
                        case '|':
                            this._pipes.Add((x, y), PipeType.NS);
                            break;
                        case '-':
                            this._pipes.Add((x, y), PipeType.EW);
                            break;
                        case 'L':
                            this._pipes.Add((x, y), PipeType.NE);
                            break;
                        case 'J':
                            this._pipes.Add((x, y), PipeType.NW);
                            break;
                        case '7':
                            this._pipes.Add((x, y), PipeType.SW);
                            break;
                        case 'F':
                            this._pipes.Add((x, y), PipeType.SE);
                            break;
                        case 'S':
                            start = (x, y);
                            startFound = true;
                            break;
                    }
                }
            }
            if(!startFound)
            {
                throw new Exception("Start not found");
            }
            if (this.SouthConnected((start.x, start.y - 1)) && this.NorthConnected((start.x, start.y + 1)))
            {
                this._possibleStartPipes.Add(PipeType.NS);
            }
            if(this.EastConnected((start.x - 1, start.y)) && this.WestConnected((start.x + 1, start.y)))
            {
                this._possibleStartPipes.Add(PipeType.EW);
            }
            if(this.SouthConnected((start.x, start.y - 1)) && this.EastConnected((start.x - 1, start.y)))
            {
                this._possibleStartPipes.Add(PipeType.NW);
            }
            if(this.SouthConnected((start.x, start.y - 1)) && this.WestConnected((start.x + 1, start.y)))
            {
                this._possibleStartPipes.Add(PipeType.NE);
            }
            if(this.NorthConnected((start.x, start.y + 1)) && this.EastConnected((start.x - 1, start.y)))
            {
                this._possibleStartPipes.Add(PipeType.SW);
            }
            if(this.NorthConnected((start.x, start.y + 1)) && this.WestConnected((start.x + 1, start.y)))
            {
                this._possibleStartPipes.Add(PipeType.SE);
            }

            if(this._possibleStartPipes.Count == 0)
            {
                throw new Exception("Start not connected");
            }
            this._startPos = start;
        }

        private bool NorthConnected((int x, int y) pos)
        {
            if (!this._pipes.ContainsKey(pos))
            {
                return false;
            }
            return this._pipes[pos] == PipeType.NS || this._pipes[pos] == PipeType.NE || this._pipes[pos] == PipeType.NW;
        }

        private bool SouthConnected((int x, int y) pos)
        {
            if (!this._pipes.ContainsKey(pos))
            {
                return false;
            }
            return this._pipes[pos] == PipeType.NS || this._pipes[pos] == PipeType.SE || this._pipes[pos] == PipeType.SW;
        }

        private bool EastConnected((int x, int y) pos)
        {
            if (!this._pipes.ContainsKey(pos))
            {
                return false;
            }
            return this._pipes[pos] == PipeType.EW || this._pipes[pos] == PipeType.NE || this._pipes[pos] == PipeType.SE;
        }

        private bool WestConnected((int x, int y) pos)
        {
            if(!this._pipes.ContainsKey(pos))
            {
                return false;
            }
            return this._pipes[pos] == PipeType.EW || this._pipes[pos] == PipeType.NW || this._pipes[pos] == PipeType.SW;
        }
    }
}
