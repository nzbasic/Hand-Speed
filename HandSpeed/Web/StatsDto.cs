using Swan.Formatters;

namespace HandSpeed.Web;

public class StatsDto
{
    public StatsDto(string distance, string speed, bool refresh)
    {
        Distance = distance;
        Speed = speed;
        Refresh = refresh;
    }

    [JsonProperty("distance")] public string Distance { get; set; }

    [JsonProperty("speed")] public string Speed { get; set; }
    
    [JsonProperty("refresh")] public bool Refresh { get; set; }
}