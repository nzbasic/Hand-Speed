using EmbedIO.WebSockets;

namespace HandSpeed.Web;

public class WebSocketsDataModule : WebSocketModule
{
    public WebSocketsDataModule(string urlPath)
        : base(urlPath, true)
    {
        AddProtocol("json");
    }

    protected override Task OnMessageReceivedAsync(
        IWebSocketContext context,
        byte[] rxBuffer,
        IWebSocketReceiveResult rxResult)
    {
        return SendToOthersAsync(context, Encoding.GetString(rxBuffer));
    }

    private Task SendToOthersAsync(IWebSocketContext context, string payload)
    {
        return BroadcastAsync(payload, c => c != context);
    }

    public async Task BroadcastEvent(string data)
    {
        await BroadcastAsync(data).ConfigureAwait(false);
    }
}