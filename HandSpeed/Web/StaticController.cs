using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.WebApi;

namespace HandSpeed.Web;

public class StaticController : WebApiController
{
    private Style? _style;
    private string? _uri;
    private string? _wsRoute;
    private int _clearInterval;

    public void Initialize(string uri, string wsRoute, Style? style, int clearInterval)
    {
        _uri = uri;
        _wsRoute = wsRoute;
        _style = style;
        _clearInterval = clearInterval;
    }

    [Route(HttpVerbs.Get, "/")]
    public async Task GetData()
    {
        await using var writer = HttpContext.OpenResponseText();
        await writer.WriteAsync(Static.Html(_uri + _wsRoute, _style, _clearInterval));
        HttpContext.Response.ContentType = "text/html";
        HttpContext.SetHandled();
    }
}