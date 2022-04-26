using System.Diagnostics;
using EmbedIO;
using OpenTabletDriver.Plugin;
using Swan.Formatters;

namespace HandSpeed.Web;

public static class WebOverlay
{
    public const string Protocol = "http://";
    public const string WsRoute = "/data";

    private static WebServer? _server;
    private static WebSocketsDataModule? _socket;

    public static void Up(string uri, Style? style, int clearInterval)
    {
        Log.Debug("Hand Speed", "Starting web overlay at " + uri);

        try
        {
            _socket = new WebSocketsDataModule(WsRoute);
            _server = new WebServer(o => o
                    .WithUrlPrefix(Protocol + uri)
                    .WithMode(HttpListenerMode.EmbedIO))
                .WithCors()
                .WithModule(_socket)
                .WithWebApi("/", m => m.RegisterController(() =>
                {
                    var api = new StaticController();
                    api.Initialize(uri, WsRoute, style, clearInterval);
                    return api;
                }));
            _server.RunAsync();

            var browser = new Process
            {
                StartInfo = new ProcessStartInfo(Protocol + uri) { UseShellExecute = true }
            };
            browser.Start();
        }
        catch (Exception e)
        {
            Log.Debug("Hand Speed", e.Message);
        }
    }

    public static void UpdateData(StatsDto dto)
    {
        var json = Json.Serialize(dto);
        _socket?.BroadcastEvent(json);
    }

    public static void Down()
    {
        Log.Debug("Hand Speed", "Shutting down web overlay");
        _server?.Dispose();
        _server = null;
    }
}