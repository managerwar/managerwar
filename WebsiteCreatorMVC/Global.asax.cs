using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Caching;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using WebsiteCreatorMVC.Models;

namespace WebsiteCreatorMVC
{
    public class MvcApplication : System.Web.HttpApplication
    {
        private static CacheItemRemovedCallback OnCacheRemove = null;

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            ApplicationDbContext.InitializeDatabase();


            CheckRound();
            AddTask("checkround", 30);

        } // Application_Start

        private void AddTask(string name, int seconds)
        {
            OnCacheRemove = new CacheItemRemovedCallback(CacheItemRemoved);
            HttpRuntime.Cache.Insert(name, seconds, null,
                DateTime.Now.AddSeconds(seconds), Cache.NoSlidingExpiration,
                CacheItemPriority.NotRemovable, OnCacheRemove);

        } // AddTask

        private void CheckRound()
        {
            // Check the round to start
            ApplicationDbContext db = new ApplicationDbContext();
            if (db.Rounds.Count() > 0)
            {
                var round = db.Rounds.OrderByDescending(q => q.ID).First();
                if (DateTime.UtcNow.Subtract(round.StartTime).TotalHours > 6)
                {
                    // Pay the last round winners
                    round.PayWinners(ref db);
                    // The round is ended create a new one
                    Round.CreateRound();
                }
            }
            else
            {
                // There is no Round create one.
                Round.CreateRound();
            }

        } // CheckRound

        public void CacheItemRemoved(string k, object v, CacheItemRemovedReason r)
        {
            if (k == "checkround")
            {
                CheckRound();
            }

            AddTask(k, Convert.ToInt32(v));

        } // CacheItemRemoved

        protected void Application_Error(object sender, EventArgs e)
        {
            string error = "";
            error += Server.GetLastError().ToString();
            SendEmail.Send(error, false, "ManagerWar error", "managerwar@yandex.com");

        } // Application_Error

    }
}
