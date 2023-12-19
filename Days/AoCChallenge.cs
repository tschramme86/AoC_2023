using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AoC2023.Days
{
    internal interface AoCChallenge
    {
        int Day { get; }
        string Name { get; }
        bool TestPartOne();
        bool TestPartTwo();
        void SolvePartOne();
        void SolvePartTwo();
        bool SkipPartOne { get; }
    }
}
