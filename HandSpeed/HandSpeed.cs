using System.Numerics;
using System.Text.RegularExpressions;
using HandSpeed.Web;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.Output;
using OpenTabletDriver.Plugin.Tablet;

namespace HandSpeed;

[PluginName("Hand Speed")]
public class CustomFilter : IPositionedPipelineElement<IDeviceReport>
{
    private readonly SpeedPoints _speedPoints = new();
    private Vector2 _lastMmPos = Vector2.Zero;
    private long _lastTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
    private Vector2 _mmScale = Vector2.Zero;
    private long _tick;
    private float _totalDistance;
    private float _cumDistance;

    [Property("Speed Formatting")]
    [DefaultPropertyValue("{0:N1}{1}")]
    public string SpeedFormat
    {
        // Remove old /s for compatibility with new formatting
        set => UnitConversion.SpeedFormatting = value.Replace("/s", "");
    }

    [Property("Distance Formatting")]
    [DefaultPropertyValue("{0:N}{1}")]
    public string DistanceFormat
    {
        set => UnitConversion.DistanceFormatting = value;
    }

    [Property("Forced Distance Unit (e.g. km)")]
    [DefaultPropertyValue("")]
    [ToolTip("Possible values: mm, cm, m, km. Leave empty to use dynamic unit")]
    public string ForcedDistanceUnit
    {
        set
        {
            var m = Regex.Match(value, @"(km|m|cm|mm)");
            
            if (m.Success)
            {
                UnitConversion.ParseForcedDistance(m.Groups[1].ToString());
            }
            else
            {
                UnitConversion.ResetForcedDistance();
                Log.Debug("Hand Speed", value + " does not match required pattern (km|m|cm|mm), ignoring");
            }
        }
    }

    [Property("Forced Speed Unit (e.g. km/h, cm/s)")]
    [DefaultPropertyValue("")]
    [ToolTip("e.g. km/h or m/s Possible Distance values: mm, cm, m, km. Possible Time values: ms, s, min, h. Leave empty to use dynamic unit")]
    public string ForcedSpeedUnit 
    {
        set
        {
            var m = Regex.Match(value, @"(km|m|cm|mm)\/(ms|s|min|h)");

            if (m.Success)
            {
                UnitConversion.ParseForcedSpeed(m.Groups[1].ToString(), m.Groups[2].ToString());
            }
            else
            {
                UnitConversion.ResetForcedSpeed();
                Log.Debug("Hand Speed", value + " does not match required pattern (km|m|cm|mm)/(ms|s|min|h), ignoring");
            }
        }
    }
    
    [Property("Save Global Distance")]
    [DefaultPropertyValue(true)]
    [ToolTip("Save the total distance travelled to a text file (located next to the OTD application)")]
    public bool SaveGlobalDistance { get; set; }

    [Property("Open Website Automatically")]
    [DefaultPropertyValue(true)]
    [ToolTip("Open the Website when Hand Speed Viewer is initialized.")]
    public bool OpenWebsiteAutomatically { get; set; }

    [Property("Speed Window Size")]
    [DefaultPropertyValue(1000)]
    [ToolTip("Time window (ms) in which the average speed is calculated. A larger window will make the average smoother.")]
    public int RollingWindow { get; set; }

    [Property("Capture Rate Limit")]
    [DefaultPropertyValue(10)]
    [ToolTip(
        "Lower is faster. The value 10 means it will run calculations every 10th tablet update.")]
    public int RateLimit { get; set; }
    
    [Property("Clear Interval")]
    [DefaultPropertyValue(2000)]
    [ToolTip("If no movement happens in the clear interval time (ms), then the speed is set to 0.")]
    public int ClearInterval { get; set; }


    [Property("Web Url:Port")]
    [DefaultPropertyValue("localhost:8073")]
    public string ServerUri { get; set; }

    [Property("Web Bg Color")]
    [DefaultPropertyValue("#1C1B22")]
    public string BackgroundColor { get; set; }

    [Property("Web Text Color")]
    [DefaultPropertyValue("#fff")]
    public string TextColor { get; set; }

    [Property("Web Outline")]
    [DefaultPropertyValue("1px solid #414141; outline-offset: -1px")]
    public string Outline { get; set; }
    
    [Property("Web Rounding")]
    [DefaultPropertyValue("0.75rem")]
    public string BorderRounding { get; set; }

    [Property("Web Font Family")]
    [DefaultPropertyValue("Inter")]
    public string FontFamily { get; set; }

    [Property("Web Font Weight")]
    [DefaultPropertyValue("bold")]
    public string FontWeight { get; set; }

    [Property("Web Font Size")]
    [DefaultPropertyValue("3rem")]
    public string FontSize { get; set; }

    [Property("Web Title")]
    [DefaultPropertyValue("Hand Speed")]
    public string Title { get; set; }

    [Property("Web Width")]
    [DefaultPropertyValue("20rem")]
    public string Width { get; set; }

    [Property("Div Style")]
    [DefaultPropertyValue("")]
    [ToolTip("(Optional) Provide styling for the parent div in inline format (e.g. display: none;)")]
    public string CustomDivStyle { get; set; }
    
    [Property("Title Style")]
    [DefaultPropertyValue("")]
    [ToolTip("(Optional) Provide styling for the title span in inline format (e.g. display: none;)")]
    public string CustomTitleStyle { get; set; }
    
    [Property("Distance Style")]
    [DefaultPropertyValue("")]
    [ToolTip("(Optional) Provide styling for the distance span in inline format (e.g. display: none;)")]
    public string CustomDistanceStyle { get; set; }
    
    [Property("Speed Style")]
    [DefaultPropertyValue("")]
    [ToolTip("(Optional) Provide styling for the speed span in inline format (e.g. display: none;)")]
    public string CustomSpeedStyle { get; set; }

    [TabletReference]
    public TabletReference TabletReference
    {
        set
        {
            var digitizer = value.Properties.Specifications.Digitizer;
            _mmScale = new Vector2
            {
                X = digitizer.Width / digitizer.MaxX,
                Y = digitizer.Height / digitizer.MaxY
            };

            Log.Debug("Hand Speed", "Initialized");
            if (OpenWebsiteAutomatically)
            {
                Log.Debug("Hand Speed", "Opening Website");
                var style = new Style(BackgroundColor, TextColor, Outline, BorderRounding, FontFamily, FontWeight, FontSize, Width,
                    Title, CustomTitleStyle, CustomDivStyle, CustomDistanceStyle, CustomSpeedStyle);
                WebOverlay.Up(ServerUri, style, ClearInterval);
            }
        }
    }

    public event Action<IDeviceReport>? Emit;

    public void Consume(IDeviceReport value)
    {
        if (value is not IAbsolutePositionReport abs)
        {
            Emit?.Invoke(value);
            return;
        }
        _tick++;

        if (_tick % RateLimit == 0)
        {
            _tick = 0;
            var mmPos = _mmScale * abs.Position;
            var dist = Vector2.Distance(mmPos, _lastMmPos);
            var currentTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            var timeDifference = currentTime - _lastTime;
            var speed = dist / timeDifference;
            _totalDistance += dist;
            _cumDistance += dist;

            _speedPoints.Add(new SpeedPoint(speed, currentTime));
            var averageSpeed = _speedPoints.CalculateAverageSpeed(RollingWindow) * 1000;

            var distFormatted = UnitConversion.FormatString(UnitConversion.DistanceFormatting, _totalDistance, true);
            var speedFormatted = UnitConversion.FormatString(UnitConversion.SpeedFormatting, averageSpeed, false);
            WebOverlay.UpdateData(new StatsDto(distFormatted, speedFormatted, false));

            if (SaveGlobalDistance && timeDifference >= 1e3)
            {
                try
                {
                    var rawDistanceString = System.IO.File.ReadAllText("global_distance_raw.txt");
                    var success = int.TryParse(rawDistanceString, out var rawDistance);

                    if (!success)
                    {
                        Log.Debug("Hand Speed", "Failed to convert int from global_distance_raw");
                    }
                    else
                    {
                        rawDistance += (int)_cumDistance;
                        _cumDistance = 0;
                        var formatted = UnitConversion.FormatString(UnitConversion.DistanceFormatting, rawDistance, false);
                        System.IO.File.WriteAllText("global_distance_raw.txt", rawDistance.ToString());
                        System.IO.File.WriteAllText("global_distance_formatted.txt", formatted);
                    }
                }
                catch (FileNotFoundException e)
                {
                    try
                    {
                        System.IO.File.WriteAllText("global_distance_raw.txt", "0");
                    }
                    catch (Exception inner)
                    {
                        Log.Debug("Hand Speed", inner.Message);
                    }
                }
                catch (Exception e)
                {
                    Log.Debug("Hand Speed", e.Message);
                }
            }

            _lastMmPos = mmPos;
            _lastTime = currentTime;
        }
        
        Emit?.Invoke(value);
    }

    public PipelinePosition Position => PipelinePosition.PreTransform;
}