using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;

namespace SWAFramework;

public class RouteConfig
{
    public static void RegisterRoutes(RouteCollection routes)
    {
        routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
        routes.Add(new Route("framework", new Handlers.SessionInfo()));
        routes.Add(new Route("debug", new Handlers.IsDebuggerAttached()));
        routes.MapRoute(
            name: "Default",
            url: "{controller}/{action}/{id}",
            defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
        );
    }
}
