using Swan.Formatters;

namespace HandSpeed.Web;

public class StatsDto
{
    public StatsDto(string distance, string speed)
    {
        Distance = distance;
        Speed = speed;
    }

    [JsonProperty("distance")] public string Distance { get; set; }

    [JsonProperty("speed")] public string Speed { get; set; }
}