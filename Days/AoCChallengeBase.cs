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

        public virtual bool SkipPartOne { get; } = false;

        protected virtual object? ExpectedTestResultPartOne { get; } = null;
        protected virtual object? ExpectedTestResultPartTwo { get; } = null;
        protected virtual bool ExtraTestDataPartTwo { get; } = false;

        protected bool IsOnTestData { get; private set; } = false;

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
            this.IsOnTestData = true;

            var testData = GetTestDataPartOne();
            var result = SolvePartOneInternal(testData);
            if(!object.Equals(result, this.ExpectedTestResultPartOne))
            {
                Console.WriteLine($"Day {Day}: Test data failed for Part One (expected {this.ExpectedTestResultPartOne} but got {result})");
                return false;
            }
            Console.WriteLine($"Day {Day}: Test data passed for Part One, result = {result}");
            return true;
        }

        public bool TestPartTwo()
        {
            this.IsOnTestData = true;

            var testData = GetTestDataPartTwo();
            var result = SolvePartTwoInternal(testData);
            if(!object.Equals(result, this.ExpectedTestResultPartTwo))
            {
                Console.WriteLine($"Day {Day}: Test data failed for Part Two (expected {this.ExpectedTestResultPartTwo} but got {result})");
                return false;
            }
            Console.WriteLine($"Day {Day}: Test data passed for Part Two, result = {result}");
            return true;
        }

        public void SolvePartOne()
        {
            this.IsOnTestData = false;

            var inputData = GetInputData();
            var result = SolvePartOneInternal(inputData);
            Console.WriteLine($"Day {Day}: Part One result = {result}");
        }

        public void SolvePartTwo()
        {
            this.IsOnTestData = false;

            var inputData = GetInputData();
            var result = SolvePartTwoInternal(inputData);
            Console.WriteLine($"Day {Day}: Part Two result = {result}");
        }

        protected virtual object SolvePartOneInternal(string[] inputData)
        {
            return -1;
        }

        protected virtual object SolvePartTwoInternal(string[] inputData)
        {
            return -1;
        }

        protected char[,] MapInput(string[] inputData)
        {
            var result = new char[inputData[0].Length, inputData.Length];
            for (int y = 0; y < inputData.Length; y++)
            {
                for (int x = 0; x < inputData[y].Length; x++)
                {
                    result[x, y] = inputData[y][x];
                }
            }
            return result;
        }

        protected T[,] MapInput<T>(string[] inputData, Func<char, (int x, int y), T> mapFunc)
        {
            var result = new T[inputData[0].Length, inputData.Length];
            for (int y = 0; y < inputData.Length; y++)
            {
                for (int x = 0; x < inputData[y].Length; x++)
                {
                    result[x, y] = mapFunc(inputData[y][x], (x,y));
                }
            }
            return result;
        }

        protected T[,] MapInput<T>(string[] inputData, Func<char, T> mapFunc)
        {
            var result = new T[inputData[0].Length, inputData.Length];
            for (int y = 0; y < inputData.Length; y++)
            {
                for (int x = 0; x < inputData[y].Length; x++)
                {
                    result[x, y] = mapFunc(inputData[y][x]);
                }
            }
            return result;
        }


        protected (int x, int y) AddP((int x, int y) p1, (int x, int y) p2)
        {
            return (p1.x + p2.x, p1.y + p2.y);
        }
    }
}
