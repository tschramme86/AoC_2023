namespace AoC2023
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("==========================================");
            Console.WriteLine("= Advent of Code 2023                    =");
            Console.WriteLine("==========================================");
            Console.WriteLine();
            Console.WriteLine("Available challenges:");
            Console.WriteLine();
            var challenges = new List<Days.AoCChallenge>();
            foreach(var type in typeof(Program).Assembly.GetTypes())
            {
                if(type.IsClass && !type.IsAbstract && type.IsSubclassOf(typeof(Days.AoCChallengeBase)))
                {
                    var challenge = Activator.CreateInstance(type) as Days.AoCChallengeBase;
                    if(challenge != null)
                    {
                        challenges.Add(challenge);
                    }
                }
            }
            foreach(var challenge in challenges.OrderBy(c => c.Day))
            {
                Console.WriteLine($"Day {challenge.Day}: {challenge.Name}");
            }
            Console.WriteLine();
            Console.WriteLine("Enter day number to run challenge (or 'x' to quit):");
            while(true)
            {
                var input = Console.ReadLine();
                if(input == "x")
                {
                    break;
                }
                if(int.TryParse(input, out var day))
                {
                    var challenge = challenges.FirstOrDefault(c => c.Day == day);
                    if(challenge != null)
                    {
                        Console.WriteLine($"Running challenge for Day {day} ({challenge.Name})");
                        Console.WriteLine();
                        if(challenge.SkipPartOne)
                        {
                            Console.WriteLine("Skipping Part One");
                        }
                        else
                        {
                            if (challenge.TestPartOne())
                            {
                                challenge.SolvePartOne();
                            }
                        }

                        if (challenge.TestPartTwo())
                        {
                            challenge.SolvePartTwo();
                        }
                        Console.WriteLine();
                        Console.WriteLine("Enter day number to run challenge (or 'x' to quit):");
                    }
                    else
                    {
                        Console.WriteLine($"No challenge found for day {day}");
                    }
                }
                else
                {
                    Console.WriteLine($"Invalid input '{input}'");
                }
            }
        }
    }
}
