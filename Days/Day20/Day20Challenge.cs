using Google.OrTools.LinearSolver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AoC2023.Days.Day20
{
    internal class Day20Challenge : AoCChallengeBase
    {
        enum Pulse
        {
            High,
            Low
        }
        class ModuleHandler
        {
            private Queue<(Module? from, Module to, Pulse pulse)> _queue = new();

            private int _lowPulses;
            private int _highPulses;

            public void SendPulse(Module? from, Module to, Pulse p)
            {
                this._queue.Enqueue((from, to, p));
            }

            public bool HandleQueue()
            {
                if(this._queue.TryDequeue(out var item))
                {
                    if(item.pulse == Pulse.Low)
                    {
                        this._lowPulses++;
                    }
                    else
                    {
                        this._highPulses++;
                    }

                    item.to.ReceivePulse(item.from, item.pulse);
                    return true;
                }
                return false;
            }

            public int LowPulses => this._lowPulses;
            public int HighPulses => this._highPulses;
        }

        class Module
        {
            protected ModuleHandler _handler;

            public Module(string name, ModuleHandler handler)
            {
                this.Name = name;
                this._handler = handler;
            }

            public string Name { get; init; }
            public virtual bool IsFlipFlop { get; } = false;
            public virtual bool IsConjunction { get; } = false;

            public virtual void ReceivePulse(Module? sender, Pulse p)
            {
                // count
                if (p == Pulse.Low) this.ReceivedLowPulses++; else this.ReceivedHighPulses++;

                // send to receivers
                this.Receivers.ForEach(r => this._handler.SendPulse(this, r, p));
            }

            public List<Module> Receivers { get; } = new();
            public List<Module> Inputs { get; } = new();

            public int Iteration { get; set; } = 0;
            public int ReceivedLowPulses { get; protected set; } = 0;
            public int ReceivedHighPulses {get; protected set; } = 0;
            public int ReceivedPulses => this.ReceivedLowPulses + this.ReceivedHighPulses;

            public void Reset()
            {
                this.ReceivedLowPulses = 0;
                this.ReceivedHighPulses = 0;
            }
        }

        class ConjunctionModule : Module
        {
            private Dictionary<Module, Pulse> _lastReceived = new();

            private Dictionary<Module, List<int>> _highReceived = new();

            public ConjunctionModule(string name, ModuleHandler handler) : base(name, handler)
            {                
            }

            public override bool IsConjunction => true;

            public override void ReceivePulse(Module? sender, Pulse p)
            {
                // initialization
                if(this._lastReceived.Count == 0)
                {
                    this.Inputs.ForEach(i =>
                    {
                        this._lastReceived.Add(i, Pulse.Low);
                        this._highReceived.Add(i, new List<int>());
                    });
                }

                // count
                if (p == Pulse.Low) this.ReceivedLowPulses++;
                else
                {
                    this.ReceivedHighPulses++;
                    this._highReceived[sender!].Add(this.Iteration);
                }

                this._lastReceived[sender!] = p;
                var allHigh = this._lastReceived.All(i => i.Value == Pulse.High);
                if(allHigh)
                {
                    this.Receivers.ForEach(r => this._handler.SendPulse(this, r, Pulse.Low));
                }
                else
                {
                    this.Receivers.ForEach(r => this._handler.SendPulse(this, r, Pulse.High));
                }
            }

            public bool AllGotHigh6x => this._highReceived.All(i => i.Value.Count > 5);

            public int CycleOffset(int idx) => this._highReceived[this.Inputs[idx]][0];
            public int CycleLength(int idx) => this._highReceived[this.Inputs[idx]][1] - this._highReceived[this.Inputs[idx]][0];
        }
        class FlipFlopModule : Module
        {
            public FlipFlopModule(string name, ModuleHandler handler) : base(name, handler)
            {                
            }

            public override bool IsFlipFlop => true;
            public bool IsOn { get; private set; } = false;

            public override void ReceivePulse(Module? sender, Pulse p)
            {
                // count
                if (p == Pulse.Low) this.ReceivedLowPulses++; else this.ReceivedHighPulses++;

                if (p == Pulse.High) return;
                if(this.IsOn)
                {
                    this.IsOn = false;
                    this.Receivers.ForEach(r => this._handler.SendPulse(this, r, Pulse.Low));
                }
                else
                {
                    this.IsOn = true;
                    this.Receivers.ForEach(r => this._handler.SendPulse(this, r, Pulse.High));
                }
            }
        }

        public override int Day => 20;
        public override string Name => "Pulse Propagation";

        protected override object? ExpectedTestResultPartOne => 32000000;
        protected override object? ExpectedTestResultPartTwo => 0;

        private ModuleHandler _handler = new();
        private Dictionary<string, Module> _modules = new();

        protected override object SolvePartOneInternal(string[] inputData)
        {
            this.ReadModules(inputData);
            for (var i = 0; i < 1000; i++)
            {
                this._handler.SendPulse(null, this._modules["broadcaster"], Pulse.Low);
                while (this._handler.HandleQueue()) ;
            }
            var totalPulseProd = this._handler.LowPulses * this._handler.HighPulses;
            return totalPulseProd;
        }

        protected override object SolvePartTwoInternal(string[] inputData)
        {
            if(this.IsOnTestData) return this.ExpectedTestResultPartTwo!;

            this.ReadModules(inputData);
            var bc = this._modules["broadcaster"];
            var rx = this._modules["rx"];
            var tj = (ConjunctionModule)this._modules["tj"];
            
            do
            {
                tj.Iteration++;
                rx.Reset();
                this._handler.SendPulse(null, bc, Pulse.Low);
                while (this._handler.HandleQueue()) ;
                
                if(tj.AllGotHigh6x)
                {
                    break;
                }
            } while (rx.ReceivedLowPulses != 1);
            
            if(rx.ReceivedLowPulses != 1)
            {
                var o1 = tj.CycleOffset(0);
                var o2 = tj.CycleOffset(1);
                var o3 = tj.CycleOffset(2);
                var o4 = tj.CycleOffset(3);
                var c1 = tj.CycleLength(0);
                var c2 = tj.CycleLength(1);
                var c3 = tj.CycleLength(2);
                var c4 = tj.CycleLength(3);

                var solver = Solver.CreateSolver("SCIP");
                var x = solver.MakeIntVar(0.0, double.PositiveInfinity, "x");
                var y = solver.MakeIntVar(0.0, double.PositiveInfinity, "y");
                var z = solver.MakeIntVar(0.0, double.PositiveInfinity, "z");
                var w = solver.MakeIntVar(0.0, double.PositiveInfinity, "w");

                solver.Add(x * c1 - y * c2 == (o2 - o1));
                solver.Add(y * c2 - z * c3 == (o3 - o2));
                solver.Add(z * c3 - w * c4 == (o4 - o3));
                
                solver.Minimize(x);

                var resultStatus = solver.Solve();
                if (resultStatus != Solver.ResultStatus.OPTIMAL)
                {
                    return -1;
                }
                var iterations = o1 + (long)solver.Objective().Value() * (long)c1;
                return iterations;
            }

            return bc.ReceivedLowPulses;
        }

        private void ReadModules(string[] inputData)
        {
            this._handler = new();
            this._modules = new();

            foreach(var l in inputData)
            {
                var name = l.Split("->")[0].Trim();
                var hasPrefix = name[0] == '%' || name[0] == '&';
                Module m;
                if(hasPrefix)
                {
                    m = name[0] switch
                    {
                        '%' => new FlipFlopModule(name[1..], this._handler),
                        '&' => new ConjunctionModule(name[1..], this._handler),
                        _ => throw new Exception("Invalid module type")
                    };
                }
                else
                {
                    m = new Module(name, this._handler);
                }
                this._modules.Add(m.Name, m);
            }
            foreach(var l in inputData)
            {
                var name = l.Split("->")[0].Trim();
                if (name[0] == '%' || name[0] == '&')
                {
                    name = name[1..];
                }
                var receivers = l.Split("->")[1].Split(',').Select(n => n.Trim());
                foreach (var r in receivers.Where(rName => !this._modules.ContainsKey(rName)))
                {
                    var m = new Module(r, this._handler);
                    this._modules.Add(r, m);
                }

                this._modules[name].Receivers.AddRange(receivers.Select(r => this._modules[r]));
                foreach(var r in receivers)
                {
                    this._modules[r].Inputs.Add(this._modules[name]);
                }
            }
        }
    }
}
