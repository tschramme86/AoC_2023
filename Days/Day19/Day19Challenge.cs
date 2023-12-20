using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AoC2023.Days.Day19
{
    internal class Day19Challenge : AoCChallengeBase
    {
        enum WorkflowResult
        {
            Unknown,
            Accepted,
            Rejected
        }

        [System.Diagnostics.DebuggerDisplay("{From} - {To} ({Length})")]
        class Range
        {
            public long From { get; set; }
            public long To { get; set; }

            public long Length => this.To - this.From + 1;
        }

        class Part
        {
            public int x { get; set; }
            public int m { get; set; }
            public int a { get; set; }
            public int s { get; set; }
            public WorkflowResult Result { get; set; }
        }

        class WorkflowRule
        {
            public bool HasCondition { get; set; }
            public string? Property { get; set; }
            public int Value { get; set; }
            public bool IsGreater { get; set; }

            public string NextWorkflowNameWhenTrue { get; set; } = string.Empty;
            public Workflow? NextWorkflowWhenTrue { get; set; }
            public bool AcceptWhenTrue { get; set; }
            public bool RejectWhenTrue { get; set; }
        }

        class Workflow
        {
            public string? Name { get; set; }
            public int Index { get; set; }

            public List<WorkflowRule> Rules { get; set; } = new();
        }

        [System.Diagnostics.DebuggerDisplay("{WorkflowName}|{WorkflowRule} {ResultVar}={MinValue}-{MaxValue}")]
        class WorkflowTree
        {
            public string? BranchVar { get; set; }

            public bool IsGreater { get; set; }

            public int Value { get; set; }

            public string? ResultVar { get; set; }
            public int MinValue { get; set; }
            public int MaxValue { get; set; }

            public bool IsLeaf { get; set; }

            public bool IsSuccess { get; set; }

            public WorkflowTree? WhenTrue { get; set; }

            public WorkflowTree? WhenFalse { get; set; }

            public WorkflowTree? Parent { get; set; }

            public string WorkflowName { get; set; } = string.Empty;
            public int WorkflowRule { get; set; }
        }

        override public int Day => 19;
        override public string Name => "Aplenty";

        override protected object? ExpectedTestResultPartOne => 19114;
        override protected object? ExpectedTestResultPartTwo => 167409079868000L;

        private List<Part> _parts = new();
        private List<Workflow> _workflows = new();
        private Workflow? _startWorkflow = null;

        protected override object SolvePartOneInternal(string[] inputData)
        {
            this.ParseInput(inputData);
            foreach(var p in this._parts)
            {
                p.Result = this.ProcessPart(p);
            }

            var acceptedParts = this._parts.Where(p => p.Result == WorkflowResult.Accepted).ToList();
            var totalSum = acceptedParts.Sum(p => p.x + p.m + p.a + p.s);

            return totalSum;
        }

        override protected object SolvePartTwoInternal(string[] inputData)
        {
            this.ParseInput(inputData);

            var successLeafs = new List<WorkflowTree>();
            void TraverseTree(Workflow w, int ruleIdx, WorkflowTree branch)
            {
                var r = w.Rules[ruleIdx];

                if(r.HasCondition)
                {
                    branch.BranchVar = r.Property;
                    branch.IsGreater = r.IsGreater;
                    branch.Value = r.Value;
                    branch.WorkflowName = w.Name!;
                    branch.WorkflowRule = ruleIdx;

                    if(r.AcceptWhenTrue)
                    {
                        var b2 = new WorkflowTree
                        {
                            IsLeaf = true,
                            IsSuccess = true,
                            Parent = branch,
                            ResultVar = r.Property,
                            MinValue = r.IsGreater ? r.Value + 1 : 1,
                            MaxValue = r.IsGreater ? 4000 : r.Value - 1,
                            WorkflowName = w.Name!,
                            WorkflowRule = ruleIdx
                        };
                        branch.WhenTrue = b2;
                        successLeafs!.Add(b2);
                    }
                    else if(r.RejectWhenTrue)
                    {
                        var b2 = new WorkflowTree
                        {
                            IsLeaf = true,
                            IsSuccess = false,
                            Parent = branch,
                            ResultVar = r.Property,
                            MinValue = r.IsGreater ? r.Value + 1 : 1,
                            MaxValue = r.IsGreater ? 4000 : r.Value - 1,
                            WorkflowName = w.Name!,
                            WorkflowRule = ruleIdx
                        };
                        branch.WhenTrue = b2;
                    }
                    else
                    {
                        branch.WhenTrue = new WorkflowTree { 
                            Parent = branch,
                            ResultVar = r.Property,
                            MinValue = r.IsGreater ? r.Value + 1 : 1,
                            MaxValue = r.IsGreater ? 4000 : r.Value - 1
                        };
                        TraverseTree(r.NextWorkflowWhenTrue!, 0, branch.WhenTrue);
                    }
                    branch.WhenFalse = new WorkflowTree
                    {
                        Parent = branch,
                        ResultVar = r.Property,
                        MinValue = r.IsGreater ? 1 : r.Value,
                        MaxValue = r.IsGreater ? r.Value : 4000
                    };
                    TraverseTree(w, ruleIdx + 1, branch.WhenFalse);
                }
                else
                {
                    if (r.NextWorkflowWhenTrue != null)
                    {
                        TraverseTree(r.NextWorkflowWhenTrue, 0, branch);
                    }
                    else
                    {
                        branch.IsLeaf = true;
                        branch.IsSuccess = r.AcceptWhenTrue;
                        branch.WorkflowName = w.Name!;
                        branch.WorkflowRule = ruleIdx;
                        if(branch.IsSuccess)
                        {
                            successLeafs!.Add(branch);
                        }
                    }
                }
            }
            var root = new WorkflowTree();
            TraverseTree(this._startWorkflow!, 0, root);

            var totalCombinations = 0L;
            foreach(var leaf in successLeafs)
            {
                var ranges = new Dictionary<string, Range>
                {
                    { "x", new Range { From = 1, To = 4000 } },
                    { "m", new Range { From = 1, To = 4000 } },
                    { "a", new Range { From = 1, To = 4000 } },
                    { "s", new Range { From = 1, To = 4000 } }
                };
                var p = leaf;
                while(p != null)
                {
                    if(!string.IsNullOrEmpty(p.ResultVar))
                    {
                        ranges[p.ResultVar].From = Math.Max(p.MinValue, ranges[p.ResultVar].From);
                        ranges[p.ResultVar].To = Math.Min(p.MaxValue, ranges[p.ResultVar].To);
                    }
                    p = p.Parent;
                }

                totalCombinations += ranges["x"].Length * ranges["m"].Length * ranges["a"].Length * ranges["s"].Length;
            }

            return totalCombinations;
        }

        private WorkflowResult ProcessPart(Part part)
        {
            var w = this._startWorkflow;
            while(w != null)
            {
                foreach(var rule in w.Rules)
                {
                    if(rule.HasCondition)
                    {
                        var partVal = (int)typeof(Part).GetProperty(rule.Property!)?.GetValue(part)!;
                        if((rule.IsGreater && partVal > rule.Value) || (!rule.IsGreater && partVal < rule.Value))
                        {
                            if(rule.AcceptWhenTrue)
                            {
                                return WorkflowResult.Accepted;
                            }
                            else if(rule.RejectWhenTrue)
                            {
                                return WorkflowResult.Rejected;
                            }
                            else
                            {
                                w = rule.NextWorkflowWhenTrue;
                                break;
                            }
                        }
                    } else
                    {
                        if(rule.AcceptWhenTrue)
                        {
                            return WorkflowResult.Accepted;
                        }
                        else if(rule.RejectWhenTrue)
                        {
                            return WorkflowResult.Rejected;
                        }
                        else
                        {
                            w = rule.NextWorkflowWhenTrue;
                            break;
                        }
                    }
                }
            }
            return WorkflowResult.Unknown;
        }

        private static readonly Regex rxPart = new Regex(@"^\{x=(?<vx>\d+),m=(?<vm>\d+),a=(?<va>\d+),s=(?<vs>\d+)\}$");
        private static readonly Regex rxWorkflow = new Regex(@"^(?<wName>\w+)\{(?<wCases>[a-z0-9<>:,AR]+)\}$");

        private void ParseInput(string[] inputData)
        {
            this._parts.Clear();
            this._workflows.Clear();

            var isWorkflowReading = true;
            for(var l=0; l<inputData.Length; l++)
            {
                if (string.IsNullOrWhiteSpace(inputData[l]))
                {
                    isWorkflowReading = false;
                    continue;
                }

                if(isWorkflowReading)
                {
                    var match = rxWorkflow.Match(inputData[l]);
                    if(match.Success)
                    {
                        var w = new Workflow
                        {
                            Name = match.Groups["wName"].Value,
                            Index = this._workflows.Count
                        };

                        var rules = match.Groups["wCases"].Value.Split(',');
                        foreach(var r in rules)
                        {
                            var rule = new WorkflowRule();
                            if(r.Contains("<"))
                            {
                                rule.HasCondition = true;
                                rule.Property = r.Split('<')[0];
                                rule.Value = int.Parse(r.Split('<')[1].Split(':')[0]);
                                rule.IsGreater = false;
                            }
                            else if(r.Contains(">"))
                            {
                                rule.HasCondition = true;
                                rule.Property = r.Split('>')[0];
                                rule.Value = int.Parse(r.Split('>')[1].Split(':')[0]);
                                rule.IsGreater = true;
                            }
                            else
                            {
                                rule.HasCondition = false;
                                if (string.Equals(r, "A", StringComparison.InvariantCulture))
                                    rule.AcceptWhenTrue = true;
                                else if (string.Equals(r, "R", StringComparison.InvariantCulture))
                                    rule.RejectWhenTrue = true;
                                else
                                    rule.NextWorkflowNameWhenTrue = r;
                            }

                            if (rule.HasCondition)
                            {
                                var target = r.Split(':')[1];
                                if (string.Equals(target, "A", StringComparison.InvariantCulture))
                                    rule.AcceptWhenTrue = true;
                                else if (string.Equals(target, "R", StringComparison.InvariantCulture))
                                    rule.RejectWhenTrue = true;
                                else
                                    rule.NextWorkflowNameWhenTrue = target;
                            }

                            w.Rules.Add(rule);
                        }

                        this._workflows.Add(w);
                        if(w.Name == "in")
                        {
                            this._startWorkflow = w;
                        }
                    }
                }
                else
                {
                    var match = rxPart.Match(inputData[l]);
                    if(match.Success)
                    {
                        this._parts.Add(new Part
                        {
                            x = int.Parse(match.Groups["vx"].Value),
                            m = int.Parse(match.Groups["vm"].Value),
                            a = int.Parse(match.Groups["va"].Value),
                            s = int.Parse(match.Groups["vs"].Value)
                        });
                    }
                }
            }

            var wfDict = this._workflows.ToDictionary(w => w.Name!);
            foreach(var w in this._workflows)
            {
                foreach(var r in w.Rules)
                {
                    if(!string.IsNullOrWhiteSpace(r.NextWorkflowNameWhenTrue))
                    {
                        r.NextWorkflowWhenTrue = wfDict[r.NextWorkflowNameWhenTrue];
                    }
                }
            }
        }
    }
}
