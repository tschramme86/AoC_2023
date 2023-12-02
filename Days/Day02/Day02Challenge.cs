using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AoC2023.Days.Day02
{
    internal class Day02Challenge : AoCChallengeBase
    {
        static readonly Regex RxGame = new Regex(@"Game (?<game>\d+): (?<round>[a-z0-9,; ]+)", RegexOptions.Compiled);
        static readonly Regex RxRound = new Regex(@"(?<count>\d+) (?<color>\w+)", RegexOptions.Compiled);

        public override int Day => 2;
        public override string Name => "Cube Conundrum";

        protected override object ExpectedTestResultPartOne => 8;
        protected override object ExpectedTestResultPartTwo => 2286;

        protected override object SolvePartOneInternal(string[] inputData)
        {
            var cubes = new Dictionary<string, int>
            {
                { "red", 12 },
                { "green", 13 },
                { "blue", 14 }
            };

            var sumIDs = 0;
            foreach (var line in inputData)
            {
                var game = RxGame.Match(line);
                if(game.Success)
                {
                    var gameID = int.Parse(game.Groups["game"].Value);
                    var round = game.Groups["round"].Value;
                    var success = true;
                    foreach(var roundPart in round.Split(';'))
                    {
                        var roundMatch = RxRound.Match(roundPart);
                        while(roundMatch.Success)
                        {
                            var count = int.Parse(roundMatch.Groups["count"].Value);
                            var color = roundMatch.Groups["color"].Value;
                            if (!cubes.TryGetValue(color, out var cubeCount) || cubeCount < count)
                            {
                                success = false;
                                break;
                            }
                            roundMatch = roundMatch.NextMatch();
                        }
                        if (!success) break;
                    }
                    if (success)
                    {
                        sumIDs += gameID;
                    }
                }
            }

            return sumIDs;
        }

        protected override object SolvePartTwoInternal(string[] inputData)
        {
            var sumPowers = 0;
            foreach (var line in inputData)
            {
                var game = RxGame.Match(line);
                if (game.Success)
                {
                    var gameID = int.Parse(game.Groups["game"].Value);
                    var round = game.Groups["round"].Value;
                    var minCubes = new Dictionary<string, int>();
                    foreach (var roundPart in round.Split(';'))
                    {
                        var roundMatch = RxRound.Match(roundPart);
                        while (roundMatch.Success)
                        {
                            var count = int.Parse(roundMatch.Groups["count"].Value);
                            var color = roundMatch.Groups["color"].Value;
                            if(minCubes.ContainsKey(color))
                            {
                                minCubes[color] = Math.Max(minCubes[color], count);
                            }
                            else
                            {
                                minCubes[color] = count;
                            }
                            roundMatch = roundMatch.NextMatch();
                        }
                    }
                    sumPowers += minCubes.Values.Aggregate(1, (a, b) => a * b);
                }
            }

            return sumPowers;
        }
    }
}
