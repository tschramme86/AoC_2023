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

    public static (double x, double y, double z) Normalize((double x, double y, double z) v)
    {
        var len = Math.Sqrt(v.x * v.x + v.y * v.y + v.z * v.z);
        return (x: (v.x / len), y: (v.y / len), z:(v.z / len));
    }

    public static (double x, double y, double z) Multiply3D((double x, double y, double z) v, double factor)
        => (v.x * factor, v.y * factor, v.z * factor);

    public static (double x, double y, double z) Add3D((double x, double y, double z) v1, (double x, double y, double z) v2)
        => (v1.x + v2.x, v1.y + v2.y, v1.z + v2.z);
}