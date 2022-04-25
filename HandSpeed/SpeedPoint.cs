namespace HandSpeed;

public struct SpeedPoint
{
    public SpeedPoint(float speed, long time)
    {
        Speed = speed;
        Time = time;
    }

    public float Speed { get; }
    public long Time { get; }
}

public class SpeedPoints
{
    private readonly List<SpeedPoint> _list;

    public SpeedPoints()
    {
        _list = new List<SpeedPoint>();
    }

    public void Add(SpeedPoint speedPoint)
    {
        _list.Add(speedPoint);
    }

    private void CleanUpSpeedPoints(int window)
    {
        var currentTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        _list.RemoveAll(point => point.Time <= currentTime - window);
    }

    public float CalculateAverageSpeed(int window)
    {
        CleanUpSpeedPoints(window);
        if (_list.Count == 0) return 0f;
        var totalSpeed = _list.Sum(point => point.Speed);
        return totalSpeed / _list.Count;
    }
}