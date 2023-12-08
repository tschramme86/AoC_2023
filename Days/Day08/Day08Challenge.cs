using System;
using System.Text.RegularExpressions;

namespace AoC2023.Days.Day08
{
	internal class Day08Challenge : AoCChallengeBase
	{
        enum Direction
        {
            L,
            R
        }

        class Node {
            public int Id { get; init; }
            public required string Name { get; init; }
            public Node? Left { get; set; }
            public Node? Right { get; set; }

            public bool IsA { get; init; }
            public bool IsZ { get; init; }
        }

        public override int Day => 8;
        public override string Name => "Haunted Wasteland";

        protected override object? ExpectedTestResultPartOne => 6;
        protected override object? ExpectedTestResultPartTwo => 6L;
        protected override bool ExtraTestDataPartTwo => true;

        private Dictionary<string, Node> _nodes = new();
        private Direction[] _directions = new Direction[0];

        protected override object SolvePartOneInternal(string[] inputData)
        {
            this.ParseNodes(inputData);
            
            var currentNode = this._nodes["AAA"];
            var step=0;
            for(; currentNode.Name != "ZZZ"; step++) {
                if(this._directions[step % _directions.Length] == Direction.L)
                    currentNode = currentNode.Left;
                else
                    currentNode = currentNode.Right;

                System.Diagnostics.Debug.Assert(currentNode != null);
            }
            return step;
        }

        protected override object SolvePartTwoInternal(string[] inputData)
        {
            this.ParseNodes(inputData);

            var nodeList = this._nodes.Values.Where(n => n.IsA).ToList();
            var cycles = new List<(long offset, long length)>();
            foreach(var node in nodeList) {
                var step=0L;
                var cycleTracker = new Dictionary<(int nodeIndex, int dirIndex), long>();
                var currentNode = node;
                while(true) {
                    var dx = (int)(step % this._directions.Length);
                    if(currentNode.IsZ) {
                        if(cycleTracker.TryGetValue((currentNode.Id, dx), out var firstIteration)) {
                            cycles.Add((firstIteration, step - firstIteration));
                            break;
                        } else {
                            cycleTracker.Add((currentNode.Id, dx), step);
                        }
                    }
                    currentNode = this._directions[dx] == Direction.L ? currentNode.Left! : currentNode!.Right!;
                    step++;
                }
            }

            var commonSteps = cycles.Select(c => c.offset).LeastCommonMultiple();
            return commonSteps;
        }

        private static readonly Regex rxNode = new(@"(?<Name>\w+) = \((?<LeftNode>\w+), (?<RightNode>\w+)\)", RegexOptions.Compiled);
        private void ParseNodes(string[] inputData)
        {
            this._nodes.Clear();

            this._directions = inputData[0].Select(d => (Direction)Enum.Parse(typeof(Direction), "" + d)).ToArray();
            var id = 0;
            foreach(var n in inputData[2..])
            {
                var m = rxNode.Match(n);
                if(m.Success) {
                    var node = new Node { 
                        Id = id++,
                        Name = m.Groups["Name"].Value,
                        IsA = m.Groups["Name"].Value[^1] == 'A',
                        IsZ = m.Groups["Name"].Value[^1] == 'Z'
                    };
                    this._nodes.Add(node.Name, node);
                }
            }

            foreach(var n in inputData[2..])
            {
                var m = rxNode.Match(n);
                if(m.Success) {
                    var node = this._nodes[m.Groups["Name"].Value];
                    node.Left = this._nodes[m.Groups["LeftNode"].Value];
                    node.Right = this._nodes[m.Groups["RightNode"].Value];
                }
            }
        }
    }
}