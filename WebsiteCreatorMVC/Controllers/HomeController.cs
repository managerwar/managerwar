using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebsiteCreatorMVC.Models;

namespace WebsiteCreatorMVC.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ApplicationDbContext db = new ApplicationDbContext();
            var curround = db.Rounds.OrderByDescending(q => q.ID).First();
            RoundIDViewModel redfish = new RoundIDViewModel();
            RoundIDViewModel greenfish = new RoundIDViewModel();
            redfish.ID = curround.ID;
            redfish.Fish = "Squid";
            ViewData["RedFish"] = redfish;
            greenfish.ID = curround.ID;
            greenfish.Fish = "Bob";
            ViewData["GreenFish"] = greenfish;

            return View();
        }

        public ActionResult Howitwork()
        {

            return View();
        }


        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        void ReadDeposits(Round round, ref string ret, ref ApplicationDbContext db, bool Red)
        {
            var DepQuery = db.Deposits.Where(q => q.RoundID_ID == round.ID && q.Red == Red).OrderByDescending(w => w.ID);
            var DepQueryConfirmed = db.Deposits.Where(q => q.RoundID_ID == round.ID && q.Status == 1 && q.Red == Red).OrderByDescending(w => w.ID);
            var DepQueryUnconfirmed = db.Deposits.Where(q => q.RoundID_ID == round.ID && q.Status == 0 && q.Red == Red).OrderByDescending(w => w.ID);
            if (DepQueryConfirmed.Count() > 0)
                ret += DepQueryConfirmed.Sum(q => q.Amount);
            else
                ret += "0";
            ret += ",\"u\":";
            if (DepQueryUnconfirmed.Count() > 0)
                ret += DepQueryUnconfirmed.Sum(q => q.Amount);
            else
                ret += "0";
            ret += ",\"t\":" + DepQuery.Count();
            ret += ",\"payments\":[";
            if (DepQuery.Count() > 0)
            {
                // Add payments
                int c = 0;
                foreach (Deposit d in DepQuery)
                {
                    if (c > 0)
                        ret += ",";
                    c++;
                    ret += "[\"";
                    switch (d.PaymentType)
                    {
                        case 1:
                            ret += "AsMoney";
                            break;
                        case 2:
                            ret += "Bitcoin";
                            break;
                        case 3:
                            ret += "Litecoin";
                            break;
                        case 4:
                            ret += "PerfectMoney";
                            break;
                    }
                    ret += "\"," + d.Amount + "," + d.Fee + "," + d.FeePercentage + ",0," + d.Status + "]";
                }
            }
            ret += "]";
        }

        public ActionResult GetGameData()
        {
            string ret = "{\"error\":null,\"result\":{\"rounds\":[";

            // Read rounds
            ApplicationDbContext db = new ApplicationDbContext();
            var roundsQuery = db.Rounds.Take(5).OrderByDescending(q => q.ID);

            // Add rounds
            int c = 0;
            foreach (Round round in roundsQuery)
            {
                if (c > 0)
                    ret += ",";
                c++;
                ret += "{\"id\":" + round.ID + ",\"status\":";
                if (round.WinnerPayed)
                    ret += "3";
                else
                    ret += "1";
                ret += ",\"jackpot\":" + round.Jackpot + ",\"startTime\":" + round.StartTime.Ticks / 10000000 + ",\"endTime\":";
                DateTime end = round.StartTime.AddHours(6);
                ret += end.Ticks / 10000000 + ",\"remainingSeconds\":";
                if (round.WinnerPayed)
                    ret += "0";
                else
                    ret += (int)(end.Subtract(DateTime.UtcNow).TotalSeconds);
                ret += ",\"cacheSeconds\":2,\"addresses\":[{\"a\":\"Red\",\"f\":"; 
                
                // Read Red Deposits
                ReadDeposits(round, ref ret, ref db, true);
                
                ret += "},{\"a\":\"Green\",\"f\":";

                // Read Green deposits
                ReadDeposits(round, ref ret, ref db, false);
                ret += "}]}";
            }
            ret += "]}}";

            return Content(ret, "application/json");
        } 
    }
}