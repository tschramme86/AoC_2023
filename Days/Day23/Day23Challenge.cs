using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AoC2023.Days.Day23
{
    internal class Day23Challenge : AoCChallengeBase
    {
        class Tile
        {
            public int Id { get; set; }

            public int X { get; set; }
            public int Y { get; set; }

            public bool IsEndPosition { get; set; } = false;

            public (int x, int y) Position => (this.X, this.Y);

            public Tile[] Neighbours { get; set; } = new Tile[0];

            public bool IsSlope { get; set; } = false;

            public bool IsPassage { get; set; } = false;
        }

        class Hydra
        {
            private int _previousCount = 0;

            public Hydra(int totalTiles)
            {
                this._contains = new bool[totalTiles];
                this.Previous = null;
            }

            public Hydra(Hydra previous, Tile nextWayTile)
            {
                this.Previous = previous;
                this._contains = (bool[])previous._contains.Clone();
                if (this._contains[nextWayTile.Id])
                {
                    throw new Exception("Tile already in hydra");
                }
                this.LastAdded = previous.LastAdded;
                this.ComingFrom = previous.ComingFrom;
                this.Add(nextWayTile);

                this._previousCount = previous._previousCount + previous.Tiles.Count;
                this.Level = previous.Level + 1;
            }

            public bool CanAdd(Tile tile)
            {
                return !this._contains[tile.Id];
            }

            public void Add(Tile tile)
            {
                this._contains[tile.Id] = true;
                this.Tiles.Add(tile);

                this.ComingFrom = this.LastAdded;
                this.LastAdded = tile;
            }

            public int Count => this.Tiles.Count + this._previousCount;

            public int Level { get; private set; }

            public Hydra? Previous { get; set; }

            public List<Tile> Tiles { get; private set; } = new List<Tile>(64);

            public Tile? LastAdded { get; private set; }
            public Tile? ComingFrom { get; private set; }

            public List<Tile> GetFullWay()
            {
                var result = new List<Tile>();
                var current = this;
                while(current != null)
                {
                    for(int i = current.Tiles.Count - 1; i >= 0; i--)
                    {
                        result.Add(current.Tiles[i]);
                    }
                    current = current.Previous;
                }
                result.Reverse();
                return result;
            }

            private bool[] _contains;
        }

        public override int Day => 23;
        public override string Name => "A Long Walk";

        protected override object? ExpectedTestResultPartOne => 94;
        protected override object? ExpectedTestResultPartTwo => 154;

        private Dictionary<(int x, int y), Tile> _tiles = new();
        private (int x, int y) _startPosition = (0, 0);
        private (int x, int y) _endPosition = (0, 0);

        protected override object SolvePartOneInternal(string[] inputData)
        {
            this.ReadMap(inputData, false);
            var longestWay = this.FindLongestWay();

            if(longestWay == null)
            {
                throw new Exception("No way found");
            }

            return longestWay.Count - 1;
        }

        protected override object SolvePartTwoInternal(string[] inputData)
        {
            this.ReadMap(inputData, true);
            var longestWay = this.FindLongestWay();

            if (longestWay == null)
            {
                throw new Exception("No way found");
            }

            return longestWay.Count - 1;
        }

        private List<Tile>? FindLongestWay()
        {
            var lwLock = new object();
            List<Tile>? longestWay = null;

            void fnIterateWay(Hydra way, Tile tile)
            {
                if (!way.CanAdd(tile)) return;

                var newWay = new Hydra(way, tile);

                if (tile.IsEndPosition)
                {
                    var w = newWay.GetFullWay();
                    lock (lwLock)
                    {
                        if (longestWay == null || w.Count > longestWay.Count)
                        {
                            longestWay = w;
                            System.Diagnostics.Debug.WriteLine("Longest way so far: " + longestWay.Count);
                        }
                    }
                    return;
                }

                var possibleNext = new List<Tile>(4);
                if (newWay.Count == 1) possibleNext.AddRange(tile.Neighbours);
                else possibleNext.AddRange(tile.Neighbours.Where(n => n != newWay.ComingFrom));

                while(possibleNext.Count == 1)
                {
                    if (!newWay.CanAdd(possibleNext[0])) return;
                    newWay.Add(possibleNext[0]);
                    if (newWay.LastAdded!.IsEndPosition)
                    {
                        var w = newWay.GetFullWay();
                        if (longestWay == null || w.Count > longestWay.Count)
                        {
                            longestWay = w;
                            System.Diagnostics.Debug.WriteLine("Longest way so far: " + longestWay.Count);
                        }
                        return;
                    }
                    possibleNext.Clear();
                    possibleNext.AddRange(newWay.LastAdded.Neighbours.Where(n => n != newWay.ComingFrom));
                }

                if (newWay.Level < 8)
                {
                    Parallel.ForEach(possibleNext, (neighbour) =>
                    {
                        fnIterateWay(newWay, neighbour);
                    });
                }
                else
                {
                    foreach (var neighbour in possibleNext)
                    {
                        fnIterateWay(newWay, neighbour);
                    }
                }
            }

            fnIterateWay(new Hydra(this._tiles.Count), this._tiles[this._startPosition]);

            return longestWay;
        }

        private void ReadMap(string[] inputData, bool withoutSlopes)
        {
            this._tiles.Clear();
            var minY = 0;
            var maxY = inputData.Length - 1;
            var minX = 0;
            var maxX = inputData[0].Length - 1;

            var map = this.MapInput(inputData, (char c, (int x, int y) p) => {
                if (c != '#')
                {
                    if (minY == p.y) this._startPosition = p;
                    if (maxY == p.y) this._endPosition = p;
                    this._tiles.Add(p, new Tile { X = p.x, Y = p.y });
                }
                return c;
            });

            var id = 0;
            foreach (var tile in this._tiles.Values)
            {
                var neighbours = new List<Tile>();
                var isSlope = withoutSlopes ? false : map[tile.X, tile.Y] != '.';

                var hasUp = false;
                var hasDown = false;
                var hasLeft = false;
                var hasRight = false;
                if(tile.Y > minY && (!isSlope || map[tile.X, tile.Y] == '^') && this._tiles.TryGetValue((tile.X, tile.Y - 1), out var t1))
                {
                    neighbours.Add(t1);
                    hasUp = true;
                }
                if (tile.Y < maxY && (!isSlope || map[tile.X, tile.Y] == 'v') && this._tiles.TryGetValue((tile.X, tile.Y + 1), out var t2))
                {
                    neighbours.Add(t2);
                    hasDown = true;
                }
                if (tile.X < maxY && (!isSlope || map[tile.X, tile.Y] == '>') && this._tiles.TryGetValue((tile.X + 1, tile.Y), out var t3))
                {
                    neighbours.Add(t3);
                    hasRight = true;
                }
                if (tile.X > minX && (!isSlope || map[tile.X, tile.Y] == '<') && this._tiles.TryGetValue((tile.X - 1, tile.Y), out var t4))
                {
                    neighbours.Add(t4);
                    hasLeft = true;
                }
                tile.Id = id++;
                tile.Neighbours = neighbours.ToArray();
                tile.IsSlope = isSlope;
                tile.IsPassage = tile.Neighbours.Length == 2 && ((hasUp && hasDown) || (hasLeft && hasRight));
                tile.IsEndPosition = tile.Position == this._endPosition;
            }   
        }
    }
}