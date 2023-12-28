using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Intrinsics;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace AoC2023.Days.Day25
{
    internal class Day25Challenge : AoCChallengeBase
    {
        [System.Diagnostics.DebuggerDisplay("{Name}")]
        class Component
        {
            public required string Name { get; set; }

            public required int Id { get; set; }

            public HashSet<Component> Connections { get; } = new();
        }

        [System.Diagnostics.DebuggerDisplay("{C1} - {C2}")]
        class Connection
        {
            public required Component C1 { get; init; }
            public required Component C2 { get; init; }
        }

        public override int Day => 25;
        public override string Name => "Snowverload";

        protected override object? ExpectedTestResultPartOne => 54;
        protected override object? ExpectedTestResultPartTwo => 1;

        private List<Component> _components = new();
        private List<Connection> _connections = new();
        private int[,]? _mat;

        protected override object SolvePartOneInternal(string[] inputData)
        {
            this.ParseComponents(inputData);

            // Stoer-Wagner algorithm
            var n = this._components.Count;
            var mat = (int[,])this._mat!.Clone();
            var minCut = int.MaxValue;
            var minCutSet = new HashSet<int>();

            var co = new List<List<int>>(n);
            for(var i = 0; i < n; i++)
            {
                co.Add(new List<int> { i });
            }

            for (int ph = 1; ph < n; ph++)
            {
                var w = this.FirstRow(mat);
                var s = 0; var t = 0;

                for (int it = 0; it < n - ph; it++)
                { 
                    // O(V^2) -> O(E log V) with prio. queue
                    w[t] = int.MinValue;
                    s = t;
                    t = w.IndexOf(w.Max());
                    for (int i = 0; i < n; i++) w[i] += mat[t, i];
                }
                if(w[t] - mat[t, t] < minCut)
                {
                    minCut = w[t] - mat[t, t];
                    minCutSet = new HashSet<int>(co[t]);
                }

                co[s].AddRange(co[t]);

                for (int i = 0; i < n; i++) mat[s, i] += mat[t, i];
                for (int i = 0; i < n; i++) mat[i, s] = mat[s, i];
                mat[0, t] = int.MinValue;
            }

            Debug.Assert(minCut == 3);
            var prod = minCutSet.Count * (n - minCutSet.Count);
            return prod;
        }

        private List<int> FirstRow(int[,] mat)
        {
            var n = mat.GetLength(0);
            var row = new List<int>(n);
            for(var i = 0; i < n; i++)
            {
                row.Add(mat[0, i]);
            }
            return row;
        }

        private void ParseComponents(string[] inputData)
        {
            this._components.Clear();
            this._connections.Clear();

            var c = new Dictionary<string, Component>();
            Component getComponent(string name)
            {
                if(c!.TryGetValue(name, out var c0))
                {
                    return c0;
                }
                var newC = new Component { Name = name, Id = c.Count };
                c[name] = newC;
                return newC;
            }

            foreach(var l in inputData)
            {
                var p = l.Split(':');
                var c0 = getComponent(p[0]);
                foreach(var cx in p[1].Trim().Split(' '))
                {
                    var c1 = getComponent(cx);
                    c0.Connections.Add(c1);
                    c1.Connections.Add(c0);

                    this._connections.Add(new Connection { C1 = c0, C2 = c1 });
                }
            }

            this._mat = new int[c.Count, c.Count];
            foreach(var conn in this._connections)
            {
                this._mat[conn.C1.Id, conn.C2.Id] = 1;
                this._mat[conn.C2.Id, conn.C1.Id] = 1;
            }

            this._components = c.Values.ToList();
        }
    }
}
