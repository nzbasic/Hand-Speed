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

internal struct TimeUnit
{
    public TimeUnit(float ratio, string unit)
    {
        this.Unit = unit;
        this.UnitToSecondRatio = ratio;
    }
    public float UnitToSecondRatio { get; set; }
    public string Unit { get; set; }
}

internal struct ForcedUnit
{
    public ForcedUnit(float conversion, string suffix)
    {
        this.Conversion = conversion;
        this.Suffix = suffix;
    }
        
    public float Conversion { get; set; }
    
    public string Suffix { get; set; }
}

public static class UnitConversion
{
    public static string SpeedFormatting;
    public static string DistanceFormatting;
    private static ForcedUnit? forcedSpeedUnit;
    private static ForcedUnit? forcedDistanceUnit;

    private static readonly List<Unit> UnitList = new()
    {
        new(1f, 9f, "mm"),
        new(10f, 999f, "cm"),
        new(1000f, 999999f, "m"),
        new(1000000f, float.MaxValue, "km")
    };

    private static readonly List<TimeUnit> TimeUnitList = new()
    {
        new(0.001f, "ms"),
        new(1f, "s"),
        new(60f, "min"),
        new(3600f, "h"),
    };

    public static void ResetForcedSpeed()
    {
        forcedSpeedUnit = null;
    }
    
    public static void ParseForcedSpeed(string distance, string time)
    {
        var distanceUnit = UnitList.Find(i => i.Suffix == distance);
        var timeUnit = TimeUnitList.Find(i => i.Unit == time);
        forcedSpeedUnit = new ForcedUnit(distanceUnit.Min / timeUnit.UnitToSecondRatio, distance + "/" + time);
    }

    public static void ResetForcedDistance()
    {
        forcedDistanceUnit = null;
    }
    
    public static void ParseForcedDistance(string distance)
    {
        var distanceUnit = UnitList.Find(i => i.Suffix == distance);
        forcedDistanceUnit = new ForcedUnit(distanceUnit.Min, distance);
    }

    private static (float conversion, string suffix) ConvertToClosestDistanceUnit(float length)
    {
        var outputConversion = 1f;
        var outputSuffix = "mm";

        foreach (var unit in UnitList.Where(unit => length < unit.Max && length >= unit.Min))
        {
            outputSuffix = unit.Suffix;
            outputConversion = unit.Min;
        }

        return (conversion: outputConversion, suffix: outputSuffix);
    }

    public static string FormatString(string format, float value, bool isDistance)
    {
        var humanReadable = ConvertToClosestDistanceUnit(value);
        var forcedConversion = new ForcedUnit(humanReadable.conversion, humanReadable.suffix);

        if (isDistance && forcedDistanceUnit != null)
        {
            forcedConversion = forcedDistanceUnit.Value;
            
        } else if (!isDistance && forcedSpeedUnit != null)
        {
            forcedConversion = forcedSpeedUnit.Value;
        } else if (!isDistance && forcedSpeedUnit == null)
        {
            forcedConversion.Suffix += "/s";
        }

        return string.Format(format, value / forcedConversion.Conversion, forcedConversion.Suffix);
    }
}