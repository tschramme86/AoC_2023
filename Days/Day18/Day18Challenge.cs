using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AoC2023.Days.Day18
{
    internal class Day18Challenge : AoCChallengeBase
    {
        enum Direction
        {
            Left,
            Up,
            Right,
            Down
        }
        private Dictionary<Direction, (int dx, int dy)> _directions = new()
        {
            { Direction.Up, (0, -1) },
            { Direction.Down, (0, 1) },
            { Direction.Left, (-1, 0) },
            { Direction.Right, (1, 0) }
        };
        class DigInstruction
        {
            public Direction Direction { get; set; }
            public long Distance { get; set; }
            public string? Color { get; set; }
            public Direction Normal { get; set; }
        }

        [System.Diagnostics.DebuggerDisplay("({X1}, {Y1}) -> ({X2}, {Y2})")]
        class Line
        {
            public long X1 { get; set; }
            public long Y1 { get; set; }
            public long X2 { get; set; }
            public long Y2 { get; set; }
            public bool IsVertical { get; set; }
            public Direction Normal { get; set; }
        }

        class Tree
        {
            public bool IsLeaf => this.Left == null && this.Right == null;
            public long X1 { get; set; }
            public long Y1 { get; set; }
            public long X2 { get; set; }
            public long Y2 { get; set; }

            public double CenterX => (this.X1 + this.X2) / 2.0;
            public double CenterY => (this.Y1 + this.Y2) / 2.0;

            public long Area => (this.X2 - this.X1 + 1)  * (this.Y2 - this.Y1 + 1);

            public int Index { get; set; }

            public Tree? Left { get; set; }
            public Tree? Right { get; set; }
        }

        override public int Day => 18;
        public override string Name => "Lavaduct Lagoon";

        protected override object? ExpectedTestResultPartOne => 62;
        protected override object? ExpectedTestResultPartTwo => 952408144115;

        protected override object SolvePartOneInternal(string[] inputData)
        {
            // read the instructions
            var instructions = this.ReadDigInstructions(inputData);

            // build the walls
            var walls = new HashSet<(int x, int y)>();
            var p = (x: 0, y: 0);
            walls.Add(p);
            foreach(var instruction in instructions)
            {
                var (dx, dy) = _directions[instruction.Direction];
                for(var i = 0; i < instruction.Distance; i++)
                {
                    p = (p.x + dx, p.y + dy);
                    walls.Add(p);
                }
            }

            // dimensions of cave?
            var minX = walls.Min(w => w.x);
            var maxX = walls.Max(w => w.x);
            var minY = walls.Min(w => w.y);
            var maxY = walls.Max(w => w.y);
            var width = maxX - minX + 1;
            var height = maxY - minY + 1;

            // translate coordinates
            walls = walls.Select(w => (w.x - minX, w.y - minY)).ToHashSet();

            // build the cave
            var cave = new bool[width, height];
            foreach(var wall in walls)
            {
                cave[wall.x, wall.y] = true;
            }

            var allCave = new HashSet<(int x, int y)>(walls);
            for(var y = 0; y < height; y++)
            {
                for(var x = 0; x < width; x++)
                {
                    if(allCave.Contains((x, y)))
                    {
                        continue;
                    }

                    // check if this point is inside the walls
                    var insideWalls = true;
                    var nextPoints = new List<(int x, int y)> { (x, y) };
                    var visited = new HashSet<(int x, int y)> { (x, y) };
                    while(nextPoints.Count > 0)
                    {
                        var currentPoint = nextPoints[0];
                        nextPoints.RemoveAt(0);

                        foreach(var direction in _directions.Values)
                        {
                            var nextPoint = (x: currentPoint.x + direction.dx, y: currentPoint.y + direction.dy);
                            if(nextPoint.x < 0 || nextPoint.x >= width || nextPoint.y < 0 || nextPoint.y >= height)
                            {
                                insideWalls = false;
                                break;
                            }
                            if(walls.Contains(nextPoint))
                            {
                                continue;
                            }
                            if(!visited.Contains(nextPoint))
                            {
                                visited.Add(nextPoint);
                                nextPoints.Add(nextPoint);
                            }
                        }

                        if(!insideWalls)
                        {
                            break;
                        }
                    }

                    if(insideWalls)
                    {
                        foreach(var v in visited)
                        {
                            allCave.Add(v);
                        }
                    }
                }
            }

            return allCave.Count;
        }

        protected override object SolvePartTwoInternal(string[] inputData)
        {
            // read the instructions
            var instructions = this.ReadDigInstructionsP2(inputData);

            // calc the area
            return this.CalcInsideArea(instructions);
        }

        private long CalcInsideArea(List<DigInstruction> instructions)
        {
            var lines = new List<Line>();
            var p = (x: 0L, y: 0L);
            foreach (var instruction in instructions)
            {
                var (dx, dy) = _directions[instruction.Direction];
                var newP = (x: p.x + dx * instruction.Distance, y: p.y + dy * instruction.Distance);
                lines.Add(new Line
                {
                    X1 = p.x,
                    Y1 = p.y,
                    X2 = newP.x,
                    Y2 = newP.y,
                    IsVertical = instruction.Direction == Direction.Up || instruction.Direction == Direction.Down,
                    Normal = instruction.Normal
                });
                p = newP;
            }

            var t = new Tree
            {
                X1 = lines.Min(l => l.X1),
                Y1 = lines.Min(l => l.Y1),
                X2 = lines.Max(l => l.X2),
                Y2 = lines.Max(l => l.Y2)
            };
            this.BuildTree(t, lines);

            // get all leaves
            var leaves = new List<Tree>();
            var nextTrees = new List<Tree> { t };
            while (nextTrees.Count > 0)
            {
                var currentTree = nextTrees[0];
                nextTrees.RemoveAt(0);
                if (currentTree.IsLeaf)
                {
                    currentTree.Index = leaves.Count;
                    leaves.Add(currentTree);
                }
                else
                {
                    nextTrees.Add(currentTree.Left!);
                    nextTrees.Add(currentTree.Right!);
                }
            }

            var insideLeaves = new List<Tree>();
            var outsideLeaves = new List<Tree>();
            var verticalLines = lines.Where(l => l.IsVertical).ToList();
            foreach (var l in leaves)
            {
                var p1 = (x: l.CenterX, y: l.CenterY);
                var cutLines = verticalLines.Where(line => p1.y.IsBetween(line.Y1, line.Y2) && line.X1 >= p1.x).ToList();
                if (cutLines.Count % 2 == 1)
                {
                    insideLeaves.Add(l);
                }
                else
                {
                    outsideLeaves.Add(l);
                }
            }

            insideLeaves = insideLeaves.OrderBy(l => l.Y1).ThenBy(l => l.X1).ToList();
            outsideLeaves = outsideLeaves.OrderBy(l => l.Y1).ThenBy(l => l.X1).ToList();

            // debug: output the leaves as SVG
            var sb = new StringBuilder();
            sb.AppendLine($"<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"{t.X1} {t.Y1} {t.X2 - t.X1} {t.Y2 - t.Y1}\">");
            foreach (var l in insideLeaves)
            {
                sb.AppendLine($"<g>");
                sb.AppendLine($"<rect x=\"{l.X1}\" data-x2=\"{l.X2}\" y=\"{l.Y1}\" data-y2=\"{l.Y2}\" width=\"{l.X2 - l.X1}\" height=\"{l.Y2 - l.Y1}\" style=\"stroke:rgb(0,0,255);fill:rgb(0,255,0);stroke-width:0.05%\" />");
                sb.AppendLine($"<text x=\"{l.CenterX}\" y=\"{l.CenterY}\" fill=\"black\">{l.Index}</text>");
                sb.AppendLine($"</g>");
            }
            foreach (var l in outsideLeaves)
            {
                sb.AppendLine($"<g>");
                sb.AppendLine($"<rect x=\"{l.X1}\" data-x2=\"{l.X2}\" y=\"{l.Y1}\" data-y2=\"{l.Y2}\" width=\"{l.X2 - l.X1}\" height=\"{l.Y2 - l.Y1}\" style=\"stroke:rgb(0,0,255);fill:rgb(255,0,0);stroke-width:0.05%\" />");
                sb.AppendLine($"<text x=\"{l.CenterX}\" y=\"{l.CenterY}\" fill=\"black\">{l.Index}</text>");
                sb.AppendLine($"</g>");
            }
            foreach (var l in lines)
            {
                sb.AppendLine($"<line x1=\"{l.X1}\" y1=\"{l.Y1}\" x2=\"{l.X2}\" y2=\"{l.Y2}\" style=\"stroke:rgb(0,0,0);stroke-width:0.2%\" />");
            }
            sb.AppendLine("</svg>");
            File.WriteAllText("leaves.svg", sb.ToString());

            var totalArea = t.Area;
            var insideArea = insideLeaves.Sum(l => l.Area);
            var outsideArea = outsideLeaves.Sum(l => l.Area);

            return insideArea;
        }

        private void BuildTree(Tree root, List<Line> lines)
        {
            foreach(var l in lines)
            {
                this.SplitTree(root, l);
            }
        }

        private void SplitTree(Tree t, Line line)
        {
            if (!line.IsVertical && (line.Y1 <= t.Y1 || line.Y1 >= t.Y2))
            {
                return;
            }
            if (line.IsVertical && (line.X1 <= t.X1 || line.X2 >= t.X2))
            {
                return;
            }
            if (t.IsLeaf)
            {
                if(line.IsVertical)
                {
                    if (line.Normal == Direction.Left)
                    {
                        t.Left = new Tree { X1 = t.X1, Y1 = t.Y1, X2 = line.X1 - 1, Y2 = t.Y2 };
                        t.Right = new Tree { X1 = line.X1, Y1 = t.Y1, X2 = t.X2, Y2 = t.Y2 };
                    } else
                    {
                        t.Left = new Tree { X1 = t.X1, Y1 = t.Y1, X2 = line.X1, Y2 = t.Y2 };
                        t.Right = new Tree { X1 = line.X1 + 1, Y1 = t.Y1, X2 = t.X2, Y2 = t.Y2 };
                    }
                } else
                {
                    if (line.Normal == Direction.Up)
                    {
                        t.Left = new Tree { X1 = t.X1, Y1 = t.Y1, X2 = t.X2, Y2 = line.Y1 - 1 };
                        t.Right = new Tree { X1 = t.X1, Y1 = line.Y1, X2 = t.X2, Y2 = t.Y2 };
                    } else
                    {
                        t.Left = new Tree { X1 = t.X1, Y1 = t.Y1, X2 = t.X2, Y2 = line.Y1 };
                        t.Right = new Tree { X1 = t.X1, Y1 = line.Y1 + 1, X2 = t.X2, Y2 = t.Y2 };
                    }
                }
            } 
            else
            {
                this.SplitTree(t.Left!, line);
                this.SplitTree(t.Right!, line);
            }
        }

        private static readonly Regex _digInstructionRegex = new Regex(@"^(?<direction>[UDLR]) (?<distance>\d+) \((?<color>.{7})\)$", RegexOptions.Compiled);
        private List<DigInstruction> ReadDigInstructions(string[] inputData)
        {
            var instructions = new List<DigInstruction>();
            foreach(var line in inputData)
            {
                var match = _digInstructionRegex.Match(line);
                if(match.Success)
                {
                    instructions.Add(new DigInstruction
                    {
                        Direction = match.Groups["direction"].Value switch
                        {
                            "U" => Direction.Up,
                            "D" => Direction.Down,
                            "L" => Direction.Left,
                            "R" => Direction.Right,
                            _ => throw new Exception($"Invalid direction: {match.Groups["direction"].Value}")
                        },
                        Distance = int.Parse(match.Groups["distance"].Value),
                        Color = match.Groups["color"].Value
                    });
                }
            }
            return instructions;
        }

        private Dictionary<(Direction d1, Direction d2), int> _turns = new()
        {
            {(Direction.Right,Direction.Up), -1 },
            {(Direction.Right,Direction.Down), 1 },
            {(Direction.Up, Direction.Left), -1 },
            {(Direction.Up, Direction.Right), 1 },
            {(Direction.Left, Direction.Down), -1 },
            {(Direction.Left, Direction.Up), 1 },
            {(Direction.Down, Direction.Right), -1 },
            {(Direction.Down, Direction.Left), 1 }  
        };

        private List<DigInstruction> ReadDigInstructionsP2(string[] inputData)
        {
            var instructions = new List<DigInstruction>();
            var normal = Direction.Left;
            var lastDirection = Direction.Up;
            foreach (var line in inputData)
            {
                var match = _digInstructionRegex.Match(line);
                if (match.Success)
                {
                    var clr = match.Groups["color"].Value;
                    var distance = clr[1..6];
                    var direction = clr[6];
                    var di = new DigInstruction
                    {
                        Direction = direction switch
                        {
                            '3' => Direction.Up,
                            '1' => Direction.Down,
                            '2' => Direction.Left,
                            '0' => Direction.Right,
                            _ => throw new Exception($"Invalid direction: {direction}")
                        },
                        Distance = Convert.ToInt64($"0x{distance}", 16),
                        Color = match.Groups["color"].Value
                    };

                    var turn = this._turns[(lastDirection, di.Direction)];
                    normal = (Direction)(((int)normal + turn + 4) % 4);
                    di.Normal = normal;
                    lastDirection = di.Direction;

                    instructions.Add(di);
                }
            }
            return instructions;
        }
    }
}
