namespace HandSpeed.Web;

public class Style
{
    public Style(string bgColor, string textColor, string outline, string rounding, string fontFamily, string fontWeight,
        string fontSize, string width, string title, string titleStyle, string divStyle, string distanceStyle,
        string speedStyle)
    {
        BgColor = bgColor;
        TextColor = textColor;
        Outline = outline;
        Rounding = rounding;
        FontFamily = fontFamily;
        FontWeight = fontWeight;
        FontSize = fontSize;
        Width = width;
        Title = title;
        TitleStyle = titleStyle;
        DivStyle = divStyle;
        DistanceStyle = distanceStyle;
        SpeedStyle = speedStyle;
    }

    public string BgColor { get; }
    public string TextColor { get; }
    public string Outline { get; }
    public string Rounding { get; }
    public string FontFamily { get; }
    public string FontWeight { get; }
    public string FontSize { get; }
    public string Width { get; }
    public string Title { get; }
    public string TitleStyle { get; }
    public string DivStyle { get; }
    public string DistanceStyle { get; }
    public string SpeedStyle { get; }
}