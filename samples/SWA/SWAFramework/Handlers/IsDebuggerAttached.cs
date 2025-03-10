using System.Web.Routing;
using System.Web.SessionState;
using System.Web;
using System.Net;

namespace SWAFramework.Handlers;

public class IsDebuggerAttached : IHttpHandler, IRouteHandler
{
    public bool IsReusable => true;

    public IHttpHandler GetHttpHandler(RequestContext requestContext) => this;

    public void ProcessRequest(HttpContext context)
    {
        if (System.Diagnostics.Debugger.IsAttached)
        {
            context.Response.StatusCode = (int)HttpStatusCode.NoContent;
        }
        else
        {
            context.Response.StatusCode = (int)HttpStatusCode.NotFound;
        }
        context.Response.End();
    }
}
