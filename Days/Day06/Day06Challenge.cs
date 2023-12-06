using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AoC2023.Days.Day06
{
    internal class Day06Challenge : AoCChallengeBase
    {
        [DebuggerDisplay("Duration: {Duration}, Record: {DistanceRecord}, WonRaces: {WonRaces}")]
        class Race
        {
            public int Duration { get; set; }
            public int DistanceRecord { get; set; }

            public long WonRaces { get; set; }
        }

        override public int Day => 6;
        override public string Name => "Wait For It";
        
        protected override object? ExpectedTestResultPartOne => 288L;
        protected override object? ExpectedTestResultPartTwo => 71503L;
        
        protected override object SolvePartOneInternal(string[] inputData)
        {
            var races = this.ParseRaces(inputData);
            foreach(var race in races)
            {
                race.WonRaces = this.WonRaces(race.Duration, race.DistanceRecord);
            }
            return races.Aggregate(1L, (n, r) => n * r.WonRaces);
        }

        override protected object SolvePartTwoInternal(string[] inputData)
        {
            var raceTime = long.Parse(inputData[0][11..].Replace(" ", string.Empty));
            var currentRecord = long.Parse(inputData[1][11..].Replace(" ", string.Empty));

            return this.WonRaces(raceTime, currentRecord);
        }

        private long WonRaces(long raceTime, long currentRecord)
        {
            var p = -raceTime;
            var q = currentRecord;
            var n1 = -(p / 2d) - Math.Sqrt((p / 2d) * (p / 2d) - q);
            var n2 = -(p / 2d) + Math.Sqrt((p / 2d) * (p / 2d) - q);

            var winningHoldFrom = (int)Math.Floor(n1) + 1;
            var winningHoldTo = (int)Math.Ceiling(n2) - 1;
            var winningRaces = winningHoldTo - winningHoldFrom + 1;

            return winningRaces;
        }

        private List<Race> ParseRaces(string[] inputData)
        {
            var results = new List<Race>();
            var times = inputData[0][11..].Split(' ').Where(n => !string.IsNullOrWhiteSpace(n)).Select(int.Parse).ToArray();
            var recs = inputData[1][11..].Split(' ').Where(n => !string.IsNullOrWhiteSpace(n)).Select(int.Parse).ToArray();

            Debug.Assert(times.Length == recs.Length);

            for(var i=0; i<times.Length; i++)
            {
                results.Add(new Race { Duration = times[i], DistanceRecord = recs[i] });
            }

            return results;
        }
    }
}
