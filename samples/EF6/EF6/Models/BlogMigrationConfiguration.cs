using System.Data.Entity.Migrations;

namespace EF6WebApp.Models
{
    public class BlogMigrationConfiguration : DbMigrationsConfiguration<BlogContext>
    {
        public BlogMigrationConfiguration()
        {
            this.AutomaticMigrationsEnabled = true;
        }
    }
}