using System.Numerics;
using System.Runtime.CompilerServices;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.Output;
using OpenTabletDriver.Plugin.Tablet;

namespace HandSpeed;

[PluginName("Hand Speed")]
public class CustomFilter : IPositionedPipelineElement<IDeviceReport>
{
    private readonly SpeedPoints _speedPoints = new();
    private float _totalDistance = 0;
    private Vector2 _mmScale = Vector2.Zero;
    private Vector2 _lastMmPos = Vector2.Zero;
    private long _lastTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
    private long _tick = 0;

    public event Action<IDeviceReport>? Emit;

    public void Consume(IDeviceReport value)
    {
        if (value is not IAbsolutePositionReport abs) return;
        _tick++;

        if (_tick % rateLimit == 0)
        {
            _tick = 0;
            var mmPos = _mmScale * abs.Position;
            var dist = Vector2.Distance(mmPos, _lastMmPos);
            var currentTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            var timeDifference = currentTime - _lastTime;
            var speed = dist / timeDifference;
            _totalDistance += dist;
            
            _speedPoints.Add(new SpeedPoint(speed, currentTime));
            var averageSpeed = _speedPoints.CalculateAverageSpeed(rollingWindow) * 1000;

            if (enableSpeedOutput) UnitConversion.WriteUnitToFile(speedOutputFile, speedFormat, averageSpeed);
            if (enableDistOutput) UnitConversion.WriteUnitToFile(distOutputFile, distFormat, _totalDistance);
            
            _lastMmPos = mmPos;
            _lastTime = currentTime;
        }
    }

    public PipelinePosition Position => PipelinePosition.PreTransform;
    
    [Property("Enable Speed Output"), DefaultPropertyValue(false)]
    public bool enableSpeedOutput { get; set; }
    
    [Property("Speed Output File"), DefaultPropertyValue("speed.txt")]
    public string speedOutputFile { get; set; }

    [Property("Speed Formatting"), DefaultPropertyValue("{0:N}{1}/s")]
    public string speedFormat { get; set; }
    
    
    [Property("Enable Distance Output"), DefaultPropertyValue(false)]
    public bool enableDistOutput { get; set; }
    
    [Property("Distance Output File"), DefaultPropertyValue("distance.txt")]
    public string distOutputFile { get; set; }
    
    [Property("Distance Formatting"), DefaultPropertyValue("{0:N}{1}")]
    public string distFormat { get; set; }
    
    [Property("Average Speed Rolling Window Size (ms)"), DefaultPropertyValue(1000)]
    public int rollingWindow { get; set; }
    

    [Property("Capture Rate Limit"), DefaultPropertyValue(10),
     ToolTip(
         "Adjust how often the speed is calculated to prevent lag. Lower is faster. The value 10 means it will run calculations every 10th update.")]
    public int rateLimit { get; set; }

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
        }
    }
}