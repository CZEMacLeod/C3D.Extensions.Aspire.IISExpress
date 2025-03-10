using System.Linq;
using System.Text.Json;
using System.Web;
using System.Web.Routing;
using System.Web.SessionState;

namespace SWAFramework.Handlers;

public class SessionInfo : IHttpHandler, IRequiresSessionState, IRouteHandler
{
    public bool IsReusable => true;

    public IHttpHandler GetHttpHandler(RequestContext requestContext) => this;

    public void ProcessRequest(HttpContext context)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = 200;

        var data = context.Session.Keys.Cast<string>().Select(key => new { Key = key, Value = context.Session[key] });

        context.Response.Write(JsonSerializer.Serialize(data));
    }
}