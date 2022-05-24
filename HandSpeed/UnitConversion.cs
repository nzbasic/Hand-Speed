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

    public static string SpeedFormatting;
    public static string DistanceFormatting;
    private static readonly List<Unit> UnitList = new()
    {
        new(1f, 9f, "mm"),
        new(10f, 999f, "cm"),
        new(1000f, 999999f, "m"),
        new(1000000f, float.MaxValue, "km")
    };

    private static (float length, string suffix) ConvertToClosestDistanceUnit(float length, bool showMetres)
    {
        var outputLength = length;
        var outputSuffix = "mm";

        foreach (var unit in UnitList.Where(unit => length < unit.Max && length >= unit.Min))
        {
            outputSuffix = unit.Suffix;
            outputLength = length / unit.Min;
        }

        if (showMetres || outputSuffix != "m") return (length: outputLength, suffix: outputSuffix);
        return (length: outputLength * 100, suffix: "cm");
    }

    public static string FormatString(string format, float value, bool showMetres)
    {
        var humanReadable = ConvertToClosestDistanceUnit(value, showMetres);
        return string.Format(format, humanReadable.length, humanReadable.suffix);
    }
}