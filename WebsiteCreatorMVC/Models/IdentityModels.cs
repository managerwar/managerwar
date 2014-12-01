using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using WebsiteCreatorMVC.Migrations;

namespace WebsiteCreatorMVC.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {
        public ApplicationUser()
        {
            Balance = 0;
        }

        public string AsmoneyAccount { get; set; }
        public string BitcoinAddress { get; set; }
        public string LitecoinAddress { get; set; }
        public string PerfectMoney { get; set; }

        [Column(TypeName="money")]
        public decimal Balance { get; set; }

    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("DefaultConnection")
        {
        }

        public DbSet<Round> Rounds { get; set; }
        
        public DbSet<Deposit> Deposits { get; set; }
        
        public DbSet<AppSettings> Settings { get; set; }

        private static readonly Object syncObj = new Object();
        public static bool InitializeDatabase()
        {
            //Database.SetInitializer<ApplicationDbContext>(null);
            lock (syncObj)
            {
                using (var temp = new ApplicationDbContext())
                {
                    if (temp.Database.Exists()) return true;

                    var initializer = new MigrateDatabaseToLatestVersion<ApplicationDbContext, Configuration>();
                    Database.SetInitializer(initializer);
                    try
                    {
                        temp.Database.Initialize(true);
                        return true;
                    }
                    catch (Exception ex)
                    {
                        //Handle Error in some way
                        return false;
                    }
                }
            }
            return true;
        }
    }
}