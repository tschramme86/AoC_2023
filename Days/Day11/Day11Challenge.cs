using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AoC2023.Days.Day11
{
    internal class Day11Challenge : AoCChallengeBase
    {
        public override int Day => 11;
        public override string Name => "Cosmic Expansion";

        protected override object? ExpectedTestResultPartOne => 374L;

        protected override object? ExpectedTestResultPartTwo => 1030L;

        private List<(long x, long y)> _galaxies = new();
        private long _width = 0;
        private long _height = 0;

        protected override object SolvePartOneInternal(string[] inputData)
        {
            this.ReadGalaxies(inputData);
            this.ExpandUniverse(2);
            var pairs = this.GetGalaxyPairs();

            // count distance between pairs
            var sumDistance = 0L;
            foreach(var pair in pairs)
            {
                var distance = Math.Abs(pair.a.x - pair.b.x) + Math.Abs(pair.a.y - pair.b.y);
                sumDistance += distance;
            }

            return sumDistance;
        }

        protected override object SolvePartTwoInternal(string[] inputData)
        {
            this.ReadGalaxies(inputData);
            this.ExpandUniverse(this.IsOnTestData ? 10 : 1000000);
            var pairs = this.GetGalaxyPairs();

            // count distance between pairs
            var sumDistance = 0L;
            foreach (var pair in pairs)
            {
                var distance = Math.Abs(pair.a.x - pair.b.x) + Math.Abs(pair.a.y - pair.b.y);
                sumDistance += distance;
            }

            return sumDistance;
        }

        private List<((long x, long y) a, (long x, long y) b)> GetGalaxyPairs()
        {
            var pairs = new List<((long x, long y) a, (long x, long y) b)>();
            for (var i = 0; i < this._galaxies.Count; i++)
            {
                for (var j = i + 1; j < this._galaxies.Count; j++)
                {
                    pairs.Add((this._galaxies[i], this._galaxies[j]));
                }
            }
            return pairs;
        }

        private void ExpandUniverse(long factor)
        {
            // expand empty rows
            for (var y = 0L; y < this._height; y++)
            {
                if(this._galaxies.Any(g => g.y == y))
                {
                    continue;
                }
                var movedGalaxies = new List<(long x, long y)>();
                foreach (var galaxy in this._galaxies.Where(g => g.y > y).ToList())
                {
                    this._galaxies.Remove(galaxy);
                    movedGalaxies.Add((galaxy.x, galaxy.y + (factor - 1)));
                }
                this._galaxies.AddRange(movedGalaxies);
                this._height += (factor - 1);
                y += (factor - 1);
            }

            // expand empty columns
            for (var x = 0L; x < this._width; x++)
            {
                if (this._galaxies.Any(g => g.x == x))
                {
                    continue;
                }
                var movedGalaxies = new List<(long x, long y)>();
                foreach (var galaxy in this._galaxies.Where(g => g.x > x).ToList())
                {
                    this._galaxies.Remove(galaxy);
                    movedGalaxies.Add((galaxy.x + (factor - 1), galaxy.y));
                }
                this._galaxies.AddRange(movedGalaxies);
                this._width += (factor - 1);
                x += (factor - 1);
            }
        }

        private void ReadGalaxies(string[] inputData)
        {
            _galaxies.Clear();
            _width = 0;
            _height = 0;

            for (var y = 0; y < inputData.Length; y++)
            {
                var line = inputData[y];
                for (var x = 0; x < line.Length; x++)
                {
                    if (line[x] == '#')
                    {
                        _galaxies.Add((x, y));
                        _width = Math.Max(_width, x);
                        _height = Math.Max(_height, y);
                    }
                }
            }
        }
    }
}
