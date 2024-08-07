using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Data.Common;
using System.Data.Entity;

namespace EF6WebApp.Models;

[DbConfigurationType(typeof(System.Data.Entity.SqlServer.MicrosoftSqlDbConfiguration))]
public class BlogContext : DbContext
{
    public BlogContext([FromKeyedServices("BloggingContext")] DbConnection existingConnection, bool contextOwnsConnection, ILogger<BlogContext> logger) :
        base(existingConnection, contextOwnsConnection)
    {
        Database.Log = m => logger.LogTrace("SqlTrace {message}", m);
    }

    public virtual DbSet<Blog> Blogs => Set<Blog>();
}
