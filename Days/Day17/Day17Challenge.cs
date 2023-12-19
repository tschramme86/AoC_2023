using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AoC2023.Days.Day17
{
    internal class Day17Challenge : AoCChallengeBase
    {
        enum Direction
        {
            Init,
            Up,
            Down,
            Left,
            Right
        }

        private Dictionary<Direction, (int x, int y)> _directions = new()
        {
            { Direction.Up, (0, -1) },
            { Direction.Down, (0, 1) },
            { Direction.Left, (-1, 0) },
            { Direction.Right, (1, 0) }
        };

        private Dictionary<Direction, List<(int x, int y, Direction d)>> _moves = new()
        {
            { Direction.Up, new List<(int x, int y, Direction d)> { (1, 0, Direction.Right), (0, -1, Direction.Up), (-1, 0, Direction.Left) } },
            { Direction.Down, new List<(int x, int y, Direction d)> { (0, 1, Direction.Down), (1, 0, Direction.Right), (-1, 0, Direction.Left) } },
            { Direction.Left, new List<(int x, int y, Direction d)> { (0, 1, Direction.Down), (0, -1, Direction.Up), (-1, 0, Direction.Left) } },
            { Direction.Right, new List<(int x, int y, Direction d)> { (1, 0, Direction.Right), (0, 1, Direction.Down), (0, -1, Direction.Up) } }
        };

        public override int Day => 17;

        public override string Name => "Clumsy Crucible";

        protected override object? ExpectedTestResultPartOne => 102;
        protected override object? ExpectedTestResultPartTwo => 94;

        protected override object SolvePartOneInternal(string[] inputData)
        {
            var heatMap = this.MapInput(inputData, c => c - '0');
            var lines = new List<(int x, int y, int heatLoss, List<(int x, int y, Direction d)> moves)> { (0, 0, 0, new List<(int x, int y, Direction d)> { (0, 0, Direction.Init), (0, 0, Direction.Init), (0, 0, Direction.Init) }) };
            var visited = new Dictionary<(int x, int y, Direction d3, Direction d2, Direction d1), int>
            {
                { (0, 0, Direction.Init, Direction.Init, Direction.Init), 0 }
            };
            var targetLines = new List<(int heatLoss, List<(int x, int y, Direction d)>)>();
            (int x, int y) target = (heatMap.GetLength(0) - 1, heatMap.GetLength(1) - 1);

            // generate one possible path to have an upper bound for the heat loss
            {
                var dPattern = new[] { Direction.Right, Direction.Right, Direction.Down, Direction.Down };
                var heatLoss = 0;
                var moves = new List<Direction> { Direction.Init, Direction.Init, Direction.Init };
                (int x, int y) p = (0, 0);
                var idx = 0;
                do
                {
                    var d = dPattern[idx++ % 4];
                    var newPos = this.AddP(p, _directions[d]);
                    heatLoss += heatMap[newPos.x, newPos.y];
                    moves.Add(d);
                    p = newPos;
                    visited[(p.x, p.y, moves[^3], moves[^2], moves[^1])] = heatLoss;
                } while (p.x < heatMap.GetLength(0) - 1);
                dPattern = new[] { Direction.Down, Direction.Left, Direction.Down, Direction.Right };
                idx = 0;
                do
                {
                    var d = dPattern[idx++ % 4];
                    var newPos = this.AddP(p, _directions[d]);
                    heatLoss += heatMap[newPos.x, newPos.y];
                    moves.Add(d);
                    p = newPos;
                    visited[(p.x, p.y, moves[^3], moves[^2], moves[^1])] = heatLoss;
                } while (p.y < heatMap.GetLength(1) - 1 || p.x < heatMap.GetLength(0) - 1);
                targetLines.Add((heatLoss, moves.Select(m => (p.x, p.y, m)).ToList()));
            }

            while(true)
            {
                var newLines = new List<(int x, int y, int heatLoss, List<(int x, int y, Direction d)> moves)>();
                foreach(var l in lines)
                {
                    var pDirection = l.moves[^1].d;
                    if (pDirection == Direction.Init) pDirection = Direction.Right;

                    foreach (var m in _moves[pDirection])
                    {
                        var newPos = this.AddP((l.x, l.y), (m.x, m.y));

                        // don't move outside of the map
                        if (newPos.x < 0 || newPos.y < 0 || newPos.x >= heatMap.GetLength(0) || newPos.y >= heatMap.GetLength(1))
                        {
                            continue;
                        }

                        // don't run in circles
                        if (l.moves.Any(prevMove => prevMove.x == newPos.x && prevMove.y == newPos.y))
                        {
                            continue;
                        }

                        // when there have been already 3 moves in the same direction, don't continue in that direction
                        if (l.moves.Count >= 3 && l.moves.TakeLast(3).All(prevMove => prevMove.d == m.d))
                        {
                            continue;
                        }

                        // check if target position has already been reached with less heat loss
                        var newHeatLoss = l.heatLoss + heatMap[newPos.x, newPos.y];
                        if (targetLines.Any(tl => tl.heatLoss <= newHeatLoss))
                        {
                            continue;
                        }

                        var moveList = l.moves.Append((newPos.x, newPos.y, m.d)).ToList();
                        if (visited.TryGetValue((newPos.x, newPos.y, moveList[^3].d, moveList[^2].d, moveList[^1].d), out var prevHeatLoss) && prevHeatLoss <= newHeatLoss)
                        {
                            continue;
                        }
                        visited[(newPos.x, newPos.y, moveList[^3].d, moveList[^2].d, moveList[^1].d)] = newHeatLoss;
                        

                        // did we reach the target?
                        if (newPos.x == target.x && newPos.y == target.y)
                        {
                            targetLines.Add((newHeatLoss, moveList));
                        } else
                        {
                            newLines.Add((newPos.x, newPos.y, newHeatLoss, moveList));
                        }
                    }
                }
                lines = newLines;

                if(lines.Count == 0)
                {
                    break;
                }
            }

            var minHeatLoss = targetLines.Min(tl => tl.heatLoss);
            return minHeatLoss;
        }

        protected override object SolvePartTwoInternal(string[] inputData)
        {
            var heatMap = this.MapInput(inputData, c => c - '0');
            var lines = new List<(int x, int y, int heatLoss, List<(int x, int y, Direction d)> moves)> { 
                (0, 0, 0, new List<(int x, int y, Direction d)> {
                    (0, 0, Direction.Init),
                    (0, 0, Direction.Init),
                    (0, 0, Direction.Init),
                    (0, 0, Direction.Init),
                    (0, 0, Direction.Init),
                    (0, 0, Direction.Init),
                    (0, 0, Direction.Init),
                    (0, 0, Direction.Init),
                    (0, 0, Direction.Init),
                    (0, 0, Direction.Init) 
                }) 
            };
            var visited = new Dictionary<(int x, int y, Direction d10, Direction d9, Direction d8, Direction d7, Direction d6, Direction d5, Direction d4, Direction d3, Direction d2, Direction d1), int>
            {
                { (0, 0, Direction.Init, Direction.Init, Direction.Init, Direction.Init, Direction.Init, Direction.Init, Direction.Init, Direction.Init, Direction.Init, Direction.Init), 0 }
            };
            var targetLines = new List<(int heatLoss, List<(int x, int y, Direction d)>)>();
            (int x, int y) target = (heatMap.GetLength(0) - 1, heatMap.GetLength(1) - 1);

            // generate one possible path to have an upper bound for the heat loss
            /*
            {
                var heatLoss = 0;
                var moves = new List<Direction> { Direction.Init, Direction.Init, Direction.Init, Direction.Init, Direction.Init, Direction.Init, Direction.Init, Direction.Init, Direction.Init, Direction.Init };
                (int x, int y) p = (0, 0);
                var d = Direction.Right;
                var mC = 0;
                do
                {
                    if(++mC == 5 || (d == Direction.Right && p.x == target.x) || (d == Direction.Down && p.y == target.y))
                    {
                        d = d == Direction.Right ? Direction.Down : Direction.Right;
                        mC = 1;
                    }

                    var newPos = this.AddP(p, _directions[d]);
                    heatLoss += heatMap[newPos.x, newPos.y];
                    moves.Add(d);
                    p = newPos;
                    visited[(p.x, p.y, moves[^10], moves[^9], moves[^8], moves[^7], moves[^6], moves[^5], moves[^4], moves[^3], moves[^2], moves[^1])] = heatLoss;
                } while (p.x < heatMap.GetLength(0) - 1 || p.y < heatMap.GetLength(1) - 1);
                targetLines.Add((heatLoss, moves.Select(m => (p.x, p.y, m)).ToList()));
            }
            */

            while (true)
            {
                var newLines = new List<(int x, int y, int heatLoss, List<(int x, int y, Direction d)> moves)>();
                foreach (var l in lines)
                {
                    var allowedDirections = new List<Direction>();
                    var hadFourMoves = l.moves[^1].d == l.moves[^2].d && l.moves[^2].d == l.moves[^3].d && l.moves[^3].d == l.moves[^4].d;
                    var hadTenMoves = hadFourMoves && l.moves[^4].d == l.moves[^5].d && l.moves[^5].d == l.moves[^6].d && l.moves[^6].d == l.moves[^7].d && l.moves[^7].d == l.moves[^8].d && l.moves[^8].d == l.moves[^9].d && l.moves[^9].d == l.moves[^10].d;
                    if (l.moves[^1].d == Direction.Init)
                    {
                        allowedDirections.Add(Direction.Right);
                        allowedDirections.Add(Direction.Down);
                    } else
                    {
                        switch (l.moves[^1].d)
                        {
                            case Direction.Up:
                                if (!hadTenMoves)
                                    allowedDirections.Add(Direction.Up);
                                if (hadFourMoves)
                                {
                                    allowedDirections.Add(Direction.Right);
                                    allowedDirections.Add(Direction.Left);
                                }
                                break;
                            case Direction.Down:
                                if (!hadTenMoves)
                                    allowedDirections.Add(Direction.Down);
                                if (hadFourMoves)
                                {
                                    allowedDirections.Add(Direction.Right);
                                    allowedDirections.Add(Direction.Left);
                                }
                                break;
                            case Direction.Left:
                                if (!hadTenMoves)
                                    allowedDirections.Add(Direction.Left);
                                if (hadFourMoves)
                                {
                                    allowedDirections.Add(Direction.Down);
                                    allowedDirections.Add(Direction.Up);
                                }
                                break;
                            case Direction.Right:
                                if (!hadTenMoves)
                                    allowedDirections.Add(Direction.Right);
                                if (hadFourMoves)
                                {
                                    allowedDirections.Add(Direction.Down);
                                    allowedDirections.Add(Direction.Up);
                                }
                                break;
                        }
                    }

                    foreach (var d in allowedDirections)
                    {
                        var newPos = this.AddP((l.x, l.y), this._directions[d]);

                        // don't move outside of the map
                        if (newPos.x < 0 || newPos.y < 0 || newPos.x >= heatMap.GetLength(0) || newPos.y >= heatMap.GetLength(1))
                        {
                            continue;
                        }

                        // don't run in circles
                        if (l.moves.Any(prevMove => prevMove.x == newPos.x && prevMove.y == newPos.y))
                        {
                            continue;
                        }

                        // check if target position has already been reached with less heat loss
                        var newHeatLoss = l.heatLoss + heatMap[newPos.x, newPos.y];
                        if (targetLines.Any(tl => tl.heatLoss <= newHeatLoss))
                        {
                            continue;
                        }

                        var moveList = l.moves.Append((newPos.x, newPos.y, d)).ToList();
                        if (visited.TryGetValue((newPos.x, newPos.y, moveList[^10].d, moveList[^9].d, moveList[^8].d, moveList[^7].d, moveList[^6].d, moveList[^5].d, moveList[^4].d, moveList[^3].d, moveList[^2].d, moveList[^1].d), out var prevHeatLoss) && prevHeatLoss <= newHeatLoss)
                        {
                            continue;
                        }
                        visited[(newPos.x, newPos.y, moveList[^10].d, moveList[^9].d, moveList[^8].d, moveList[^7].d, moveList[^6].d, moveList[^5].d, moveList[^4].d, moveList[^3].d, moveList[^2].d, moveList[^1].d)] = newHeatLoss;


                        // did we reach the target?
                        if (newPos.x == target.x && newPos.y == target.y)
                        {
                            targetLines.Add((newHeatLoss, moveList));
                        }
                        else
                        {
                            newLines.Add((newPos.x, newPos.y, newHeatLoss, moveList));
                        }
                    }
                }
                lines = newLines;

                if (lines.Count == 0)
                {
                    break;
                }
            }

            var minHeatLoss = targetLines.Min(tl => tl.heatLoss);
            return minHeatLoss;
        }
    }
}
