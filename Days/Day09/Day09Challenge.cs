namespace AoC2023.Days.Day09
{
    internal class Day09Challenge : AoCChallengeBase
    {
        public override int Day => 9;
        public override string Name => "Mirage Maintenance";

        protected override object? ExpectedTestResultPartOne => 114;
        protected override object? ExpectedTestResultPartTwo => 2;

        protected override object SolvePartOneInternal(string[] inputData)
        {
            var numberLines = inputData.Select(l => l.Split(' ').Select(int.Parse).ToList()).ToList();
            foreach (var line in numberLines)
            {
                var deductedLines = new List<List<int>> { line };
                var currentLine = line;
                do
                {
                    var newLine = new List<int>();
                    for (var i = 0; i < currentLine.Count - 1; i++)
                        newLine.Add(currentLine[i + 1] - currentLine[i]);
                    deductedLines.Add(newLine);
                    currentLine = newLine;
                } while(currentLine.Any(n => n != 0));

                currentLine.Add(0);
                for(var d=deductedLines.Count-2; d>=0; d--) {
                    deductedLines[d].Add(deductedLines[d][^1] + deductedLines[d+1][^1]);
                }
            }

            return numberLines.Select(l => l[^1]).Sum();
        }

        protected override object SolvePartTwoInternal(string[] inputData)
        {
            var numberLines = inputData.Select(l => l.Split(' ').Select(int.Parse).ToList()).ToList();
            foreach (var line in numberLines)
            {
                var deductedLines = new List<List<int>> { line };
                var currentLine = line;
                do
                {
                    var newLine = new List<int>();
                    for (var i = 0; i < currentLine.Count - 1; i++)
                        newLine.Add(currentLine[i + 1] - currentLine[i]);
                    deductedLines.Add(newLine);
                    currentLine = newLine;
                } while(currentLine.Any(n => n != 0));

                currentLine.Add(0);
                for(var d=deductedLines.Count-2; d>=0; d--) {
                    deductedLines[d].Insert(0, deductedLines[d][0] - deductedLines[d+1][0]);
                }
            }

            return numberLines.Select(l => l[0]).Sum();
        }
    }
}