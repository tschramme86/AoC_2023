using System.Numerics;
namespace AoC2023;

public static class MathHelpers
{
    public static T GreatestCommonDivisor<T>(T a, T b) where T : INumber<T>
    {
        while (b != T.Zero)
        {
            var temp = b;
            b = a % b;
            a = temp;
        }

        return a;
    }

    public static T LeastCommonMultiple<T>(T a, T b) where T : INumber<T>
        => a / GreatestCommonDivisor(a, b) * b;
    
    public static T LeastCommonMultiple<T>(this IEnumerable<T> values) where T : INumber<T>
        => values.Aggregate(LeastCommonMultiple);

    public static bool IsBetween<T>(this T value, T v1, T v2) where T : INumber<T>
    {
        var min = v1 < v2 ? v1 : v2;
        var max = v1 > v2 ? v1 : v2;

        return value >= min && value <= max;
    }
}