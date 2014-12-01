using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebsiteCreatorMVC.Models
{
    public class Round
    {
        public Round()
        {
            TotalDeposit = 0;
            TotalFee = 0;
            WinnerPayed = false;
        }

        [Key]
        public long ID { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime StartTime { get; set; }
        
        [Column(TypeName="money")]
        public decimal TotalDeposit { get; set; }

        [Column(TypeName="money")]
        public decimal TotalFee { get; set; }

        public bool WinnerPayed { get; set; }

        [Column(TypeName="money")]
        public decimal Jackpot { get; set; }

        public static void CreateRound()
        {
            Round r = new Round();

            // Find the time.
            DateTime now = DateTime.UtcNow;
            DateTime t0 = new DateTime(now.Year, now.Month, now.Day, 6, 0, 0);
            if (DateTime.Compare(now, t0) > 0 && now.Subtract(t0).TotalHours < 6)
                r.StartTime = t0;
            else
            {
                DateTime t1 = new DateTime(now.Year, now.Month, now.Day, 12,0, 0);
                if (DateTime.Compare(now, t1) > 0 && now.Subtract(t1).TotalHours < 6)
                    r.StartTime = t1;
                else
                {
                    DateTime t2 = new DateTime(now.Year, now.Month, now.Day, 18, 0, 0);
                    if (DateTime.Compare(now, t2) > 0 && now.Subtract(t2).TotalHours < 6)
                        r.StartTime = t2;
                    else
                    {
                        DateTime t3 = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0);
                        if (DateTime.Compare(now, t3) > 0 && now.Subtract(t3).TotalHours < 6)
                            r.StartTime = t3;
                        else
                        {
                            // Somthing wrong happend
                            throw new Exception("Wrong date for creating round confused.");
                        }
                    }
                }
            }
            var db = new ApplicationDbContext();

            // TODO: For now the jackpot is 1$
            var TotalProfitObj = db.Settings.Where(q => q.Name == "TotalProfit").First();
            decimal TotalProfit = Convert.ToDecimal(TotalProfitObj.Value);
            r.Jackpot = TotalProfit * (decimal)0.025;
            if (r.Jackpot < 1)
                r.Jackpot = 1;
            TotalProfit -= r.Jackpot;
            TotalProfitObj.Value = TotalProfit.ToString();

            // add it to database
            db.Rounds.Add(r);
            db.SaveChanges();

        } // CreateRound

        public void PayWinners(ref ApplicationDbContext db)
        {

            // TODO: Calculate the winners
            var RDepQueryConfirmed = db.Deposits.Where(q => q.RoundID_ID == ID && q.Status == 1 && q.Red == true);
            var GDepQueryConfirmed = db.Deposits.Where(q => q.RoundID_ID == ID && q.Status == 1 && q.Red == false);

            decimal RT = 0;
            decimal GT = 0; 

            if ( RDepQueryConfirmed.Count() > 0)
                RT = RDepQueryConfirmed.Sum(q => q.Amount);
            if (GDepQueryConfirmed.Count() > 0)
                GT = GDepQueryConfirmed.Sum(q => q.Amount);

            IQueryable<Deposit> winners;
            decimal WinTotal;
            decimal LoseTotal;
            if (RT > GT)
            {
                winners = RDepQueryConfirmed;
                WinTotal = RT;
                LoseTotal = GT + RT + Jackpot;
            }
            else
            {
                winners = GDepQueryConfirmed;
                WinTotal = GT;
                LoseTotal = GT + RT + Jackpot;
            }

            asmoneyapi.AsmoneyAPI api = new asmoneyapi.AsmoneyAPI("managerwar", "api", "1532648sx");
            PerfectMoney pmapi = new PerfectMoney();
            int txid = 0;
            foreach (Deposit d in winners)
            {
                decimal share = d.Amount / WinTotal;
                decimal win = share * LoseTotal;
                d.User = db.Users.Where(q => q.Id == d.User_Id).First();

                // send money
                d.User.Balance += win;
                if (!string.IsNullOrEmpty(d.User.AsmoneyAccount))
                {
                    api.Transfer(d.User.AsmoneyAccount, (double)win, "USD", "Win " + win + " USD in round #" + d.RoundID_ID, out txid);
                }
                else if (!string.IsNullOrEmpty(d.User.BitcoinAddress))
                {
                    api.TransferBTC(d.User.BitcoinAddress, (double)win, "USD", "Win " + win + " USD in round #" + d.RoundID_ID, out txid);
                }
                else if (!string.IsNullOrEmpty(d.User.LitecoinAddress))
                {
                    api.TransferLTC(d.User.LitecoinAddress, (double)win, "USD", "Win " + win + " USD in round #" + d.RoundID_ID, out txid);
                }
                else if (!string.IsNullOrEmpty(d.User.PerfectMoney))
                {
                    int t = (int)(win * 100);
                    double a = (double)t / 100;
                    Dictionary<string, string> r = pmapi.Transfer("8701096", "159159sx", "U8289470", d.User.PerfectMoney, a, 0, 0);
                    if (r.Keys.Contains("ERROR"))
                    {
                        SendEmail.Send("Send " + a + " USD to " + d.User.PerfectMoney + " Username is " + d.User.UserName + " Error is " + r["ERROR"], false, "Need PM", "managerwar@yandex.com");
                    }
                }

            }

            WinnerPayed = true;

            // Add fees to total profit
            var TotalProfitObj = db.Settings.Where(q => q.Name == "TotalProfit").First();
            decimal TotalProfit = Convert.ToDecimal(TotalProfitObj.Value);
            
            // Check if site profit is more than 100$
            if (TotalProfit > 100)
            {
                TotalProfit += TotalFee / 2;
                // Send 50% of fee to my user                
                api.Transfer("cyrus", (double)TotalFee / 2, "USD", "50% fee of round " + ID, out txid);
            }
            else
                TotalProfit += TotalFee;

            TotalProfitObj.Value = TotalProfit.ToString();

            db.SaveChanges();

        } // PayWinners

    } // Round

    public class RoundIDViewModel
    {
        public long ID { get; set; }
        public string Fish { get; set; }

    } // RoundIDViewModel
}