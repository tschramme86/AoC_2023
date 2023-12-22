using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace AoC2023.Days.Day22
{
    internal class Day22Challenge : AoCChallengeBase
    {
        [System.Diagnostics.DebuggerDisplay("{Name} ({P1}-{P2}) {VOffset} S:{IsSettled} Z:{MinZ}-{MaxZ}")]
        class Brick
        {
            private (int x, int y, int z) _p1;
            private (int x, int y, int z) _p2;
            private List<(int x, int y, int z)> _pList;

            public Brick((int x, int y, int z) p1, (int x, int y, int z) p2)
            {
                this._p1 = p1;
                this._p2 = p2;

                var dx = Math.Sign(p2.x - p1.x);
                var dy = Math.Sign(p2.y - p1.y);
                var dz = Math.Sign(p2.z - p1.z);
                var pList = new List<(int x, int y, int z)> { p1 };
                var p = p1;
                while(p != p2)
                {
                    p = (p.x + dx, p.y + dy, p.z + dz);
                    pList.Add(p);
                }
                this._pList = pList;

                this.IsSettled = this.MinZ == 1;
            }

            public string Name { get; set; } = string.Empty;

            public int VOffset { get; set; }

            public (int x, int y, int z) P1 => (this._p1.x, this._p1.y, this._p1.z + this.VOffset);
            public (int x, int y, int z) P2 => (this._p2.x, this._p2.y, this._p2.z + this.VOffset);

            public IEnumerable<(int x, int y, int z)> PList => this._pList.Select(p => (p.x, p.y, p.z + this.VOffset));

            public int MinZ => Math.Min(this._p1.z + this.VOffset, this._p2.z + this.VOffset);
            public int MaxZ => Math.Max(this._p1.z + this.VOffset, this._p2.z + this.VOffset);

            public bool IsSettled { get; set; } = false;

            public bool IsSupportedBy(Brick other)
            {
                return other.IsSupporting(this);
            }

            public bool IsSupporting(Brick other)
            {
                if(other == this) return false;
                if(other == null) return false;

                if(this.MaxZ + 1 != other.MinZ)
                {
                    return false;
                }

                var myPoints = this.PList.ToHashSet();
                foreach (var p in other.PList)
                {
                    if(myPoints.Contains((p.x, p.y, p.z - 1)))
                    {
                        return true;
                    }
                }

                return false;
            }

            public List<Brick> SupportedByList { get; } = new List<Brick>();
            public List<Brick> SupportingList { get; } = new List<Brick>();
        }

        public override int Day => 22;
        public override string Name => "Sand Slabs";

        protected override object? ExpectedTestResultPartOne => 5;
        protected override object? ExpectedTestResultPartTwo => 7;

        private List<Brick> _brickList = new();

        protected override object SolvePartOneInternal(string[] inputData)
        {
            this.ParseBricks(inputData);
            this.LetTheBricksFall();

            var bricksToBeRemoved = this._brickList.Where(b => 
                b.SupportingList.Count == 0 ||
                b.SupportingList.All(x => x.SupportedByList.Count > 1)).ToList();

            return bricksToBeRemoved.Count;
        }

        override protected object SolvePartTwoInternal(string[] inputData)
        {
            this.ParseBricks(inputData);
            this.LetTheBricksFall();

            var bricksWillStartReaction = this._brickList
                    .Where(b => b.SupportedByList.Count == 1)
                    .Select(b => b.SupportedByList[0])
                    .Distinct().ToList();

            var fallenBricksCount = new List<int>();
            foreach(var b in bricksWillStartReaction)
            {
                var fallenBricks = new HashSet<Brick> { b };
                while (true)
                {
                    var beforeIteration = fallenBricks.Count;
                    foreach(var candidateToFall in this._brickList)
                    {
                        if(candidateToFall.SupportedByList.Count > 0 && 
                            candidateToFall.SupportedByList.All(fallenBricks.Contains))
                        {
                            fallenBricks.Add(candidateToFall);
                        }
                    }
                    if(fallenBricks.Count == beforeIteration)
                    {
                        break;
                    }
                }

                fallenBricksCount.Add(fallenBricks.Count);
            }

            return fallenBricksCount.Sum(x => x - 1);
        }

        private void LetTheBricksFall()
        {
            var settledBricks = this._brickList.Where(b => b.IsSettled).ToList();
            while (settledBricks.Count < this._brickList.Count)
            {
                foreach (var brick in this._brickList.Where(b => !b.IsSettled))
                {
                    if (!this._brickList.Any(b => b.IsSupporting(brick) && brick.MinZ > 1))
                    {
                        brick.VOffset--;
                    }
                    if (settledBricks.Any(b => b.IsSupporting(brick)) || brick.MinZ == 1)
                    {
                        brick.IsSettled = true;
                        settledBricks.Add(brick);
                    }
                }
            }

            // calculate support
            foreach (var brick in this._brickList)
            {
                foreach (var other in this._brickList)
                {
                    if (brick.IsSupporting(other))
                    {
                        brick.SupportingList.Add(other);
                        other.SupportedByList.Add(brick);
                    }
                }
            }
        }

        private void ParseBricks(string[] inputData)
        {
            this._brickList.Clear();

            var l = 'A';
            foreach(var line in inputData)
            {
                var parts = line.Split('~');
                var c1 = parts[0].Split(',');
                var c2 = parts[1].Split(',');
                var p1 = (int.Parse(c1[0]), int.Parse(c1[1]), int.Parse(c1[2]));
                var p2 = (int.Parse(c2[0]), int.Parse(c2[1]), int.Parse(c2[2]));
                var brick = new Brick(p1, p2);
                if(this.IsOnTestData)
                {
                    brick.Name = l++.ToString();
                }
                this._brickList.Add(brick);
            }
        }
    }
}
