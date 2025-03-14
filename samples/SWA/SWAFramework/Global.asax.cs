using System;
using System.Configuration;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using C3D.Extensions.SystemWeb.OpenTelemetry.Application;
using OpenTelemetry.Resources;
using System.Collections.Generic;

namespace SWAFramework;

public class MvcApplication : OpenTelemeteryApplication
{

    protected override void Application_Start()
    {
        base.Application_Start();

        this.AddSystemWebAdapters()
            .AddProxySupport(options => options.UseForwardedHeaders = true)
            .AddSessionSerializer(options =>
            {
            })
            .AddJsonSessionSerializer(options =>
            {
                options.RegisterKey<int>("CoreCount");
            })
            .AddRemoteAppServer(options => options.ApiKey = ConfigurationManager.AppSettings["RemoteApp:ApiKey"])
            .AddSessionServer(options =>
            {
            });

        AreaRegistration.RegisterAllAreas();
        GlobalConfiguration.Configure(WebApiConfig.Register);
        FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
        RouteConfig.RegisterRoutes(RouteTable.Routes);
        BundleConfig.RegisterBundles(BundleTable.Bundles);
    }

    protected void Application_PostAcquireRequestState(object sender, EventArgs e)
    {
        if (((HttpApplication)sender).Context.Session is { } session)
        {
            if (session["FrameworkCount"] is int count)
            {
                session["FrameworkCount"] = count + 1;
            }
            else
            {
                session["FrameworkCount"] = 0;
            }
        }
    }

    private static readonly Dictionary<string, object> GitResourceAttributes = new() {
        { "vcs.system",      "git" },
        { "vcs.commit.id",   ThisAssembly.GitCommitId },
        { "vcs.commit.date", ThisAssembly.GitCommitDate.ToString("O") }
    };

    protected override void ConfigureResource(ResourceBuilder builder)
    {
        base.ConfigureResource(builder);
        builder.AddProcessRuntimeDetector();
        builder.AddAttributes(GitResourceAttributes);
    }
}

