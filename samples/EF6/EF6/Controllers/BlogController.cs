using Microsoft.Extensions.Logging;
using System.Data.Common;
using System.Linq;
using System.Web.Mvc;
using EF6WebApp.Models;

namespace EF6WebApp.Controllers
{
    public class BlogController : Controller
    {
        private readonly ILogger logger;
        private readonly BlogContext dbContext;

        public BlogController(ILogger<BlogController> logger, Models.BlogContext dbContext)
        {
            this.logger = logger;
            this.dbContext = dbContext;
            logger?.LogInformation("DbContext ConnectionString {connectionString}", dbContext.Database.Connection.ConnectionString);
        }

        public ActionResult Index()
        {
            var connectionString = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(dbContext.Database.Connection.ConnectionString);
            if (!string.IsNullOrEmpty(connectionString.Password))
            {
                connectionString.Password = "****";
            }
            var root = HttpContext.Server.MapPath("~/");
            if (!string.IsNullOrEmpty(connectionString.AttachDBFilename) && connectionString.AttachDBFilename.StartsWith(root, System.StringComparison.InvariantCultureIgnoreCase))
            {
                connectionString.AttachDBFilename = "~\\" + connectionString.AttachDBFilename.Substring(root.Length);
            }

            ViewBag.Message = "Connected using " +
                DbProviderFactories.GetFactory(dbContext.Database.Connection).ToString();
            ViewBag.ConnectionString = connectionString;
            var blogs = dbContext.Blogs!;
            logger?.LogInformation("{blogCount} blogs found", blogs.Count());
            return View(blogs.OrderBy(b => b.Name).ThenBy(b => b.Url).ThenBy(b => b.BlogId).ToList());
        }
    }
}
