namespace HandSpeed.Web;

// mega scuffed way of returning HTML because I couldn't figure out how to make html files work within plugins :))
public static class Static
{
    public static string Html(string url, Style? style, int clearInterval)
    {
        return $@"
            <!DOCTYPE html>
            <html lang='en'>
                <head>
                    <meta charset='UTF-8'>
                    <title>Hand Speed</title>
                </head>
                <body style=""margin: 0;display: inline-block; font-family: '{style.FontFamily}'; font-size: {style.FontSize}; font-weight: {style.FontWeight};"">
                    <div style=""background-color: {style.BgColor};color: {style.TextColor};border-radius: {style.Rounding};padding: 1rem;display: flex;flex-direction: column;justify-content: center;align-items: center;width: {style.Width}; {style.DivStyle}"">
                        <span style=""margin-bottom: 0.5rem; {style.TitleStyle}"">{style.Title}</span>
                        <span style=""{style.DistanceStyle}""id='distance'>0.00mm</span>
                        <span style=""{style.SpeedStyle}""id='speed'>0.0mm/s</span>
                    </div>
                    <script>
                        const connection = new WebSocket('ws://{url}', 'json')
                      
                        let timeout;
                        connection.onmessage = (event) => {{
                            if (timeout) clearTimeout(timeout);
                            const json = JSON.parse(event.data);
                            document.getElementById('distance').textContent = json.distance
                            document.getElementById('speed').textContent = json.speed
                            if ({clearInterval}) {{
                                timeout = setTimeout(() => {{
                                    document.getElementById('speed').textContent = ""{UnitConversion.FormatString(UnitConversion.SpeedFormatting, 0f)}""
                                }}, {clearInterval})
                            }}
                        }}
                    </script>
                </body>
            </html>
        ";
    }
}