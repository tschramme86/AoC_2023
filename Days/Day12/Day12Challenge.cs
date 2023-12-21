using System;
using System.Collections.Concurrent;
using Google.OrTools.Sat;

namespace AoC2023.Days.Day12
{
    internal class Day12Challenge : AoCChallengeBase
    {
        enum SpringCondition
        {
            Unknown,
            Operational,
            Damaged
        }

        class SpringRow
        {
            public int Index { get; set; }
            public SpringCondition[] Springs { get; set; } = new SpringCondition[0];
            public List<int> SpringGroups = new();

            public List<SpringCondition[]> Solutions { get; } = new();
        }

        class VarArraySolutionPrinter : CpSolverSolutionCallback
        {
            public VarArraySolutionPrinter(BoolVar[] variables, SpringRow sr, bool storeSolution)
            {
                this._variables = variables;
                this._springRow = sr;
                this._storeSolution = storeSolution;
            }

            public override void OnSolutionCallback()
            {
                if (this._storeSolution)
                {
                    var solution = new List<SpringCondition>();
                    foreach (var v in _variables)
                    {
                        solution.Add(this.BooleanValue(v) ? SpringCondition.Damaged : SpringCondition.Operational);
                    }
                    this._springRow.Solutions.Add(solution.ToArray());
                }
                this._solution_count++;

                if(this._solution_count % 100 == 0)
                    this.PrintStatus();
            }

            public int SolutionCount()
            {
                return _solution_count;
            }

            public void PrintStatus()
            {
                // Console.Write($"\rSolving row {this._springRow.Index}, solutions so far: {this._solution_count}");
            }

            private int _solution_count;
            private BoolVar[] _variables;
            private SpringRow _springRow;
            private bool _storeSolution;
        }

        public override int Day => 12;
        public override string Name => "Hot Springs";

        protected override object? ExpectedTestResultPartOne => 21;

        protected override object? ExpectedTestResultPartTwo => 525152L;

        protected override object SolvePartOneInternal(string[] inputData)
        {
            var possibleArrangements = 0;
            var idx = 0;
            foreach (var line in inputData)
            {
                var springRow = this.ParseSpringRow(line, false);
                springRow.Index = idx++;
                possibleArrangements += this.SolveSpringRowSATSolver(springRow);
            }
            return possibleArrangements;
        }

        protected override object SolvePartTwoInternal(string[] inputData)
        {
            var possibleArrangements = new List<long>();
            var idx = 0;

            var rows = new List<SpringRow>();
            foreach (var line in inputData)
            {
                var springRow = this.ParseSpringRow(line, true);
                springRow.Index = idx++;
                rows.Add(springRow);

                possibleArrangements.Add(this.GetPossibleArrangements(springRow.Springs, springRow.SpringGroups.ToArray()));
            }

            return possibleArrangements.Sum();
        }

        /// <summary>
        /// Adapted from: https://github.com/joeleisner/advent-of-code-2023/blob/main/days/12/mod.ts
        /// </summary>
        private Dictionary<string, long> _cache = new();
        private long GetPossibleArrangements(SpringCondition[] conditions, int[] groups)
        {
            if(conditions.Length == 0)
                return groups.Length > 0 ? 0 : 1;

            if(groups.Length == 0) 
                return conditions.Any(s => s == SpringCondition.Damaged) ? 0 : 1;
            
            var key = GetKey(conditions, groups);
            if(this._cache.ContainsKey(key))
                return this._cache[key];

            var result = 0L;

            var condition = conditions[0];
            if (condition == SpringCondition.Operational || condition == SpringCondition.Unknown)
            {
                result += this.GetPossibleArrangements(conditions.Skip(1).ToArray(), groups);
            }

            var group = groups[0];
            if(condition == SpringCondition.Damaged || condition == SpringCondition.Unknown)
            {
                if(group <= conditions.Length &&
                    !conditions.Take(group).Any(c => c == SpringCondition.Operational) &&
                    (group == conditions.Length || conditions[group] != SpringCondition.Damaged))
                {
                    result += this.GetPossibleArrangements(conditions.Skip(group + 1).ToArray(), groups.Skip(1).ToArray());
                }
            }

            this._cache.Add(key, result);
            return result;
        }

        private static string GetKey(SpringCondition[] conditions, int[] groups)
        {
            return $"{string.Join(',', conditions)}|{string.Join(',', groups)}";
        }

        private int SolveSpringRowSATSolver(SpringRow sr)
        {
            sr.Solutions.Clear();
            var model = new CpModel();

            // one bool var for each spring
            var w = sr.Springs.Length;
            var isDamagedSpringVars = Enumerable.Range(0, w).Select(x => model.NewBoolVar($"ds_{x}")).ToArray();
            var solutionCallback = new VarArraySolutionPrinter(isDamagedSpringVars, sr, false);

            // fix var if status is known
            for (var i = 0; i < w; i++)
            {
                if (sr.Springs[i] == SpringCondition.Damaged)
                    model.Add(isDamagedSpringVars[i] == 1);
                if (sr.Springs[i] == SpringCondition.Operational)
                    model.Add(isDamagedSpringVars[i] == 0);
            }

            // fix total number of damaged springs
            var totalDamagedSprings = sr.SpringGroups.Sum();
            var constr = isDamagedSpringVars[0] * 1;
            for (var i = 1; i < w; i++)
                constr = constr + isDamagedSpringVars[i] * 1;
            model.Add(constr == totalDamagedSprings);

            var springGroups = new Dictionary<int, List<(int startPos, BoolVar v)>>();
            var earliestStart = 0;
            for (var i = 0; i < sr.SpringGroups.Count; i++)
            {
                springGroups.Add(i, new List<(int startPos, BoolVar v)>());
                var l = sr.SpringGroups[i];

                var isLastGroup = i == sr.SpringGroups.Count - 1;
                var latestStart = w - (sr.SpringGroups.Skip(i).Sum() + (sr.SpringGroups.Count - i - 1));
                for (var start = earliestStart; start <= latestStart; start++)
                {
                    var bv = model.NewBoolVar($"sg_{i}_s_{start}");
                    springGroups[i].Add((start, bv));

                    // when this is the start position, imply for damaged vars
                    for (var dx = 0; dx < l; dx++)
                    {
                        model.AddImplication(bv, isDamagedSpringVars[start + dx]);
                    }

                    // since two groups of damaged springs must not be connected, there must be
                    // at least one non-damaged after this group, if it is not the last group
                    if(!isLastGroup)
                    {
                        model.AddImplication(bv, isDamagedSpringVars[start + l].Not());
                    }
                }

                // there must be exactly one start position
                model.AddExactlyOne(springGroups[i].Select(sg => sg.v));

                earliestStart += sr.SpringGroups[i] + 1;
            }
            // connect spring groups -> later group must not start before this group
            for (var i = 0; i < sr.SpringGroups.Count - 1; i++)
            {
                foreach (var spv in springGroups[i])
                {
                    var earliestStartNextGroup = spv.startPos + sr.SpringGroups[i] + 1;
                    foreach (var succeedingStart in springGroups[i + 1].Where(k => k.startPos < earliestStartNextGroup))
                    {
                        model.AddImplication(spv.v, succeedingStart.v.Not());
                    }
                }
            }

            // now solve the model
            var solver = new CpSolver();
            solver.StringParameters = "enumerate_all_solutions:true";

            solutionCallback.PrintStatus();
            var solutionStatus = solver.Solve(model, solutionCallback);
            
            solutionCallback.PrintStatus();
            // Console.WriteLine();

            return solutionCallback.SolutionCount();
        }

        private SpringRow ParseSpringRow(string input, bool expand)
        {
            var parts = input.Split(' ');

            var springStatus = parts[0];
            var springGroups = parts[1];
            if (expand)
            {
                for (var i = 0; i < 4; i++)
                {
                    springStatus = $"{springStatus}?{parts[0]}";
                    springGroups = $"{springGroups},{parts[1]}";
                }
            }

            var sr = new SpringRow
            {
                Springs = springStatus.Select(c => c switch { '.' => SpringCondition.Operational, '#' => SpringCondition.Damaged, _ => SpringCondition.Unknown }).ToArray(),
                SpringGroups = springGroups.Split(',').Select(int.Parse).ToList()
            };

            return sr;
        }
    }
}