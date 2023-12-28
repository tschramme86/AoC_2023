using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Google.OrTools.ConstraintSolver;
using Google.OrTools.ModelBuilder;
using Google.OrTools.Sat;
using Microsoft.Z3;

namespace AoC2023.Days.Day24
{
    internal class Day24Challenge : AoCChallengeBase
    {
        [System.Diagnostics.DebuggerDisplay("({Position.x}, {Position.y}, {Position.z}) -> ({Direction.x}, {Direction.y}, {Direction.z})")]
        class HailRay
        {
            public HailRay((double x, double y, double z) p, (double x, double y, double z) dir)
            {
                this.Position = p;
                this.Direction = dir;
                this.DirectionN = MathHelpers.Normalize(dir);
            }

            public (double x, double y, double z) Position { get; set; }

            public (double x, double y, double z) Direction { get; set; }

            public (double x, double y, double z) DirectionN { get; set; }

            public (double x, double y, double t1, double t2) Intersect2D(HailRay other)
            {
                var thisP2 = MathHelpers.Add3D(this.Position, MathHelpers.Multiply3D(this.Direction, 10000));
                var otherP2 = MathHelpers.Add3D(other.Position, MathHelpers.Multiply3D(other.Direction, 10000));

                var A1 = thisP2.y - this.Position.y;
                var B1 = this.Position.x - thisP2.x;
                var C1 = A1 * this.Position.x + B1 * this.Position.y;

                var A2 = otherP2.y - other.Position.y;
                var B2 = other.Position.x - otherP2.x;
                var C2 = A2 * other.Position.x + B2 * other.Position.y;

                var det = A1 * B2 - A2 * B1;
                if (det == 0)
                {
                    return (double.NaN, double.NaN, double.NaN, double.NaN);
                }
                else
                {
                    var x = (B2 * C1 - B1 * C2) / det;
                    var y = (A1 * C2 - A2 * C1) / det;

                    var t1 = (x - this.Position.x) / this.Direction.x;
                    var t2 = (x - other.Position.x) / other.Direction.x;

                    return (x, y, t1, t2);
                }
            }
        }

        public override int Day => 24;
        public override string Name => "Never Tell Me The Odds";

        protected override object? ExpectedTestResultPartOne => 2;
        protected override object? ExpectedTestResultPartTwo => 47L;

        private List<HailRay> _hails = new();

        protected override object SolvePartOneInternal(string[] inputData)
        {
            this.ParseHails(inputData);
            var testAreaMin = this.IsOnTestData ? (x: 7d, y: 7d) : (x: 200000000000000d, y: 200000000000000d);
            var testAreaMax = this.IsOnTestData ? (x: 27d, y: 27d) : (x: 400000000000000d, y: 400000000000000d);

            var possibleIntersections = new List<(HailRay a, HailRay b)>();
            for(var i = 0; i < this._hails.Count; i++)
            {
                for(var j=i+1; j < this._hails.Count;j++)
                {
                    var hailA = this._hails[i];
                    var hailB = this._hails[j];
                    var (x, y, t1, t2) = hailA.Intersect2D(hailB);
                    if (double.IsNaN(x) || double.IsNaN(y) || double.IsNaN(t1))
                    {
                        continue;
                    }

                    if (t1 >= 0 && t2 >= 0 && x.IsBetween(testAreaMin.x, testAreaMax.x) && y.IsBetween(testAreaMin.y, testAreaMax.y))
                    {
                        possibleIntersections.Add((hailA, hailB));
                    }
                }
            }

            return possibleIntersections.Count;
        }

        protected override object SolvePartTwoInternal(string[] inputData)
        {
            this.ParseHails(inputData);

            using (var ctx = new Context())
            {
                var solver = ctx.MkSolver();
                
                var stoneX = (ArithExpr)ctx.MkConst(ctx.MkSymbol("stoneX"), ctx.MkRealSort());
                var stoneY = (ArithExpr)ctx.MkConst(ctx.MkSymbol("stoneY"), ctx.MkRealSort());
                var stoneZ = (ArithExpr)ctx.MkConst(ctx.MkSymbol("stoneZ"), ctx.MkRealSort());
                var stoneDX = (ArithExpr)ctx.MkConst(ctx.MkSymbol("stoneDX"), ctx.MkRealSort());
                var stoneDY = (ArithExpr)ctx.MkConst(ctx.MkSymbol("stoneDY"), ctx.MkRealSort());
                var stoneDZ = (ArithExpr)ctx.MkConst(ctx.MkSymbol("stoneDZ"), ctx.MkRealSort());
                var g = ctx.MkGoal();

                for (var i = 0; i < 3; i++)
                {
                    var hail = this._hails[i];
                    var (hx, hy, hz) = hail.Position;
                    var (hdx, hdy, hdz) = hail.Direction;

                    var ht = (ArithExpr)ctx.MkConst(ctx.MkSymbol($"ht_{i}"), ctx.MkRealSort());
                    
                    g.Add(ctx.MkGe(ht, ctx.MkReal(0)));

                    g.Add(ctx.MkEq(ctx.MkAdd(stoneX, ctx.MkMul(stoneDX, ht)), 
                        ctx.MkAdd(ctx.MkReal((long)hx), ctx.MkMul(ht, ctx.MkReal((long)hdx)))));

                    g.Add(ctx.MkEq(ctx.MkAdd(stoneY, ctx.MkMul(stoneDY, ht)), 
                        ctx.MkAdd(ctx.MkReal((long)hy), ctx.MkMul(ht, ctx.MkReal((long)hdy)))));

                    g.Add(ctx.MkEq(ctx.MkAdd(stoneZ, ctx.MkMul(stoneDZ, ht)), 
                        ctx.MkAdd(ctx.MkReal((long)hz), ctx.MkMul(ht, ctx.MkReal((long)hdz)))));
                }

                foreach (BoolExpr a in g.Formulas)
                    solver.Assert(a);

                var status = solver.Check();
                if (status == Status.SATISFIABLE)
                {
                    var tx = solver.Model.Eval(stoneX);
                    var ty = solver.Model.Eval(stoneY);
                    var tz = solver.Model.Eval(stoneZ);

                    return 
                        long.Parse(tx.ToString()) +
                        long.Parse(ty.ToString()) + 
                        long.Parse(tz.ToString());
                }
            }

            return base.SolvePartTwoInternal(inputData);
        }

        private void ParseHails(string[] inputData)
        {
            this._hails.Clear();
            foreach (var line in inputData)
            {
                var parts = line.Split('@');
                var pos = parts[0].Trim().Split(',');
                var dir = parts[1].Trim().Split(',');
                var x = double.Parse(pos[0]);
                var y = double.Parse(pos[1]);
                var z = double.Parse(pos[2]);
                var dx = double.Parse(dir[0]);
                var dy = double.Parse(dir[1]);
                var dz = double.Parse(dir[2]);

                this._hails.Add(new HailRay((x, y, z), (dx, dy, dz)));
            }
        }
    }
}
