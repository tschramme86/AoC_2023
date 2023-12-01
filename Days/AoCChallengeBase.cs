using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AoC2023.Days
{
    internal abstract class AoCChallengeBase : AoCChallenge
    {
        public virtual int Day { get; }
        public virtual string Name { get; } = string.Empty;

        protected virtual object? ExpectedTestResultPartOne { get; } = null;
        protected virtual object? ExpectedTestResultPartTwo { get; } = null;
        protected virtual bool ExtraTestDataPartTwo { get; } = false;

        protected virtual string[] GetTestDataPartOne()
        {
            return System.IO.File.ReadAllLines($"Days/Day{Day:D2}/d{Day:D2}_input_test.txt");
        }

        protected virtual string[] GetTestDataPartTwo()
        {
            if(this.ExtraTestDataPartTwo)
            {
                return System.IO.File.ReadAllLines($"Days/Day{Day:D2}/d{Day:D2}_input_test2.txt");
            }
            return System.IO.File.ReadAllLines($"Days/Day{Day:D2}/d{Day:D2}_input_test.txt");
        }

        protected virtual string[] GetInputData()
        {
            return System.IO.File.ReadAllLines($"Days/Day{Day:D2}/d{Day:D2}_input.txt");
        }

        public bool TestPartOne()
        {
            var testData = GetTestDataPartOne();
            var result = SolvePartOneInternal(testData);
            if(!object.Equals(result, this.ExpectedTestResultPartOne))
            {
                Console.WriteLine($"Day {Day:D2}: Test data failed for Part One (expected {this.ExpectedTestResultPartOne} but got {result})");
                return false;
            }
            Console.WriteLine($"Day {Day:D2}: Test data passed for Part One, result = {result}");
            return true;
        }

        public bool TestPartTwo()
        {
            var testData = GetTestDataPartTwo();
            var result = SolvePartTwoInternal(testData);
            if(!object.Equals(result, this.ExpectedTestResultPartTwo))
            {
                Console.WriteLine($"Day {Day:D2}: Test data failed for Part Two (expected {this.ExpectedTestResultPartTwo} but got {result})");
                return false;
            }
            Console.WriteLine($"Day {Day:D2}: Test data passed for Part Two, result = {result}");
            return true;
        }

        public void SolvePartOne()
        {
            var inputData = GetInputData();
            var result = SolvePartOneInternal(inputData);
            Console.WriteLine($"Day {Day:D2}: Part One result = {result}");
        }

        public void SolvePartTwo()
        {
            var inputData = GetInputData();
            var result = SolvePartTwoInternal(inputData);
            Console.WriteLine($"Day {Day:D2}: Part Two result = {result}");
        }

        protected virtual object SolvePartOneInternal(string[] inputData)
        {
            return -1;
        }

        protected virtual object SolvePartTwoInternal(string[] inputData)
        {
            return -1;
        }
    }
}
