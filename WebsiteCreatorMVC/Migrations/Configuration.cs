namespace WebsiteCreatorMVC.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<WebsiteCreatorMVC.Models.ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
        }

        protected override void Seed(WebsiteCreatorMVC.Models.ApplicationDbContext context)
        {
            //  This method will be called after migrating to the latest version.

            context.Settings.AddOrUpdate(
                q => q.Name,
                new Models.AppSettings { Name = "TotalProfit", Value = "0" }
                );

            context.SaveChanges();
        }
    }
}
