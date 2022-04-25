using System.Numerics;

namespace HandSpeed;

internal struct Unit
{
    public Unit(float min, float max, string suffix)
    {
        Min = min;
        Max = max;
        Suffix = suffix;
    }

    public float Min { get; set; }
    public float Max { get; set; }
    public string Suffix { get; set; }
}

public static class UnitConversion
{

    private static readonly List<Unit> UnitList = new List<Unit>
    {
        new Unit(1f, 9f, "mm"),
        new Unit(10f, 999f, "cm"),
        new Unit(1000f, 999999f, "m"),
        new Unit(1000000f, float.MaxValue, "km")
    };
    
    private static (float length, string suffix) ConvertToClosestDistanceUnit(float length)
    {
        var outputLength = length;
        var outputSuffix = "mm";

        foreach (var unit in UnitList.Where(unit => length < unit.Max && length >= unit.Min))
        {
            outputSuffix = unit.Suffix;
            outputLength = length / unit.Min;
        }

        return (length: outputLength, suffix: outputSuffix);
    }
    
    public static void WriteUnitToFile(string fileName, string format, float distance)
    {
        var humanReadable = UnitConversion.ConvertToClosestDistanceUnit(distance);
        var formattedString = string.Format(format, humanReadable.length, humanReadable.suffix);
        File.WriteAllText(fileName, formattedString);
    }
}