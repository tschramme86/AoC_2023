using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AoC2023.Days.Day16
{
    internal class Day16Challenge : AoCChallengeBase
    {
        enum Direction
        {
            Up,
            Down,
            Left,
            Right
        }

        private Dictionary<Direction, (int x, int y)> _moves = new Dictionary<Direction, (int x, int y)>
        {
            { Direction.Up, (0, -1) },
            { Direction.Down, (0, 1) },
            { Direction.Left, (-1, 0) },
            { Direction.Right, (1, 0) }
        };

        public override int Day => 16;
        public override string Name => "The Floor Will Be Lava";

        protected override object? ExpectedTestResultPartOne => 46;

        protected override object? ExpectedTestResultPartTwo => 51;

        override protected object SolvePartOneInternal(string[] inputData)
        {
            var map = this.MapInput(inputData);
            return this.EnergizedTiles(map, (-1, 0, Direction.Right));
        }

        override protected object SolvePartTwoInternal(string[] inputData)
        {
            var map = this.MapInput(inputData);
            var maxEnergized = 0;
            for(var x=0; x<map.GetLength(0); x++)
            {
                maxEnergized = Math.Max(maxEnergized, this.EnergizedTiles(map, (x, -1, Direction.Down)));
                maxEnergized = Math.Max(maxEnergized, this.EnergizedTiles(map, (x, map.GetLength(1), Direction.Up)));
            }
            for(var y=0; y<map.GetLength(1); y++)
            {
                maxEnergized = Math.Max(maxEnergized, this.EnergizedTiles(map, (-1, y, Direction.Right)));
                maxEnergized = Math.Max(maxEnergized, this.EnergizedTiles(map, (map.GetLength(0), y, Direction.Left)));
            }
            return maxEnergized;
        }

        private int EnergizedTiles(char[,] map, (int x, int y, Direction d) initialBeam)
        {
            var hitTiles = new HashSet<(int x, int y)>();
            var visitedTiles = new HashSet<(int x, int y, Direction d)>();
            var heads = new List<(int x, int y, Direction d)> { initialBeam };
            do
            {
                var newHeads = new List<(int x, int y, Direction d)>();
                foreach (var h in heads)
                {
                    var p1 = AddP((h.x, h.y), _moves[h.d]);
                    if (p1.x < 0 || p1.y < 0 || p1.x >= map.GetLength(0) || p1.y >= map.GetLength(1))
                    {
                        continue;
                    }
                    hitTiles.Add(p1);
                    if ((h.d == Direction.Up || h.d == Direction.Down) && map[p1.x, p1.y] == '-')
                    {
                        var r1 = (p1.x, p1.y, Direction.Right);
                        var r2 = (p1.x, p1.y, Direction.Left);
                        if (!visitedTiles.Contains(r1))
                        {
                            newHeads.Add(r1);
                            visitedTiles.Add(r1);
                        }
                        if (!visitedTiles.Contains(r2))
                        {
                            newHeads.Add(r2);
                            visitedTiles.Add(r2);
                        }
                    }
                    else if ((h.d == Direction.Left || h.d == Direction.Right) && map[p1.x, p1.y] == '|')
                    {
                        var r1 = (p1.x, p1.y, Direction.Up);
                        var r2 = (p1.x, p1.y, Direction.Down);
                        if (!visitedTiles.Contains(r1))
                        {
                            newHeads.Add(r1);
                            visitedTiles.Add(r1);
                        }
                        if (!visitedTiles.Contains(r2))
                        {
                            newHeads.Add(r2);
                            visitedTiles.Add(r2);
                        }
                    }
                    else if (h.d == Direction.Right && map[p1.x, p1.y] == '\\')
                    {
                        var r1 = (p1.x, p1.y, Direction.Down);
                        if (!visitedTiles.Contains(r1))
                        {
                            newHeads.Add(r1);
                            visitedTiles.Add(r1);
                        }
                    }
                    else if (h.d == Direction.Left && map[p1.x, p1.y] == '\\')
                    {
                        var r1 = (p1.x, p1.y, Direction.Up);
                        if (!visitedTiles.Contains(r1))
                        {
                            newHeads.Add(r1);
                            visitedTiles.Add(r1);
                        }
                    }
                    else if (h.d == Direction.Right && map[p1.x, p1.y] == '/')
                    {
                        var r1 = (p1.x, p1.y, Direction.Up);
                        if (!visitedTiles.Contains(r1))
                        {
                            newHeads.Add(r1);
                            visitedTiles.Add(r1);
                        }
                    }
                    else if (h.d == Direction.Left && map[p1.x, p1.y] == '/')
                    {
                        var r1 = (p1.x, p1.y, Direction.Down);
                        if (!visitedTiles.Contains(r1))
                        {
                            newHeads.Add(r1);
                            visitedTiles.Add(r1);
                        }
                    }
                    else if (h.d == Direction.Up && map[p1.x, p1.y] == '\\')
                    {
                        var r1 = (p1.x, p1.y, Direction.Left);
                        if (!visitedTiles.Contains(r1))
                        {
                            newHeads.Add(r1);
                            visitedTiles.Add(r1);
                        }
                    }
                    else if (h.d == Direction.Down && map[p1.x, p1.y] == '\\')
                    {
                        var r1 = (p1.x, p1.y, Direction.Right);
                        if (!visitedTiles.Contains(r1))
                        {
                            newHeads.Add(r1);
                            visitedTiles.Add(r1);
                        }
                    }
                    else if (h.d == Direction.Up && map[p1.x, p1.y] == '/')
                    {
                        var r1 = (p1.x, p1.y, Direction.Right);
                        if (!visitedTiles.Contains(r1))
                        {
                            newHeads.Add(r1);
                            visitedTiles.Add(r1);
                        }
                    }
                    else if (h.d == Direction.Down && map[p1.x, p1.y] == '/')
                    {
                        var r1 = (p1.x, p1.y, Direction.Left);
                        if (!visitedTiles.Contains(r1))
                        {
                            newHeads.Add(r1);
                            visitedTiles.Add(r1);
                        }
                    }
                    else
                    {
                        var r1 = (p1.x, p1.y, h.d);
                        if (!visitedTiles.Contains(r1))
                        {
                            newHeads.Add(r1);
                            visitedTiles.Add(r1);
                        }
                    }
                }
                heads = newHeads;
            } while (heads.Count > 0);
            return hitTiles.Count;
        }

        private (int x, int y) AddP((int x, int y) a, (int x, int y) b)
        {
            return (a.x + b.x, a.y + b.y);
        }
    }
}
