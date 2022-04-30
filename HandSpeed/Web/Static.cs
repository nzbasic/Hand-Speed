namespace HandSpeed.Web;

// mega scuffed way of returning HTML because I couldn't figure out how to make html files work within plugins :))
// edit: its actually ok now because it acts as SSR for user styles 
public static class Static
{
    public static string Html(string url, Style style, int clearInterval)
    {
        return $@"
            <!DOCTYPE html>
            <html lang='en'>
                <head>
                    <meta charset='UTF-8'>
                    <title>Hand Speed</title>
                    <link rel=""preconnect"" href=""https://fonts.googleapis.com"">
                    <link rel=""preconnect"" href=""https://fonts.gstatic.com"" crossorigin>
                    <link href=""https://fonts.googleapis.com/css2?family=Inter:wght@100;200;300;400;500;600;700;800;900&display=swap"" rel=""stylesheet"">
                </head>
                <body style=""margin: 0;display: inline-block; font-family: '{style.FontFamily}'; font-size: {style.FontSize}; font-weight: {style.FontWeight};"">
                    <div style=""background-color: {style.BgColor};color: {style.TextColor};border-radius: {style.Rounding};padding: 1rem;display: flex;flex-direction: column;justify-content: center;align-items: center;width: {style.Width}; outline: {style.Outline}; {style.DivStyle}"">
                        <span style=""margin-bottom: 0.5rem; {style.TitleStyle}"">{style.Title}</span>
                        <span style=""{style.SpeedStyle}""id='speed'>0.0mm/s</span>
                        <span style=""{style.DistanceStyle}""id='distance'>0.00mm</span>
                    </div>
                    <script>
                        const connection = new WebSocket('ws://{url}', 'json')
                      
                        let timeout;
                        connection.onmessage = (event) => {{
                            if (timeout) clearTimeout(timeout);
                            const json = JSON.parse(event.data);
                            
                            if (json.refresh) {{
                                window.location.reload();
                                return;
                            }}

                            document.getElementById('distance').textContent = json.distance;
                            document.getElementById('speed').textContent = json.speed;
                            if ({clearInterval}) {{
                                timeout = setTimeout(() => {{
                                    document.getElementById('speed').textContent = ""{UnitConversion.FormatString(UnitConversion.SpeedFormatting, 0f)}"";
                                }}, {clearInterval});
                            }}
                        }}
                    </script>
                </body>
            </html>
        ";
    }
}