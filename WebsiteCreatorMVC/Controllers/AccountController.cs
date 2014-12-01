using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin.Security;
using WebsiteCreatorMVC.Models;

namespace WebsiteCreatorMVC.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        public AccountController()
            : this(new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new ApplicationDbContext())))
        {
            UserManager.UserValidator = new UserValidator<ApplicationUser>(UserManager) { AllowOnlyAlphanumericUserNames = false };
        }

        public AccountController(UserManager<ApplicationUser> userManager)
        {
            UserManager = userManager;
        }

        public UserManager<ApplicationUser> UserManager { get; private set; }

        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindAsync(model.Email, model.Password);
                if (user != null)
                {
                    await SignInAsync(user, model.RememberMe);
                    return RedirectToLocal(returnUrl);
                }
                else
                {
                    ModelState.AddModelError("", "Invalid username or password.");
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }

        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser() { UserName = model.Email, 
                    AsmoneyAccount = model.AsMoneyAccount,
                    BitcoinAddress = model.BitcoinAddress,
                    LitecoinAddress = model.LitecoinAddress,
                    PerfectMoney = model.PerfectMoney
                };
                var result = await UserManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    AddErrors(result);
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        [AllowAnonymous]
        public ActionResult AsMoney()
        {
            string PAYEE_ACCOUNT = Request.Params["PAYEE_ACCOUNT"];
            string PAYER_ACCOUNT = Request.Params["PAYER_ACCOUNT"];
            string PAYMENT_AMOUNT = Request.Params["PAYMENT_AMOUNT"];
            string PAYMENT_UNITS = Request.Params["PAYMENT_UNITS"];
            string BATCH_NUM = Request.Params["BATCH_NUM"];
            string PAYMENT_ID = Request.Params["PAYMENT_ID"];
            string CF_1 = Request.Params["CF_1"];
            string CF_2 = Request.Params["CF_2"];
            string MD5_HASH = Request.Params["MD5_HASH"];
            string PAYMENT_STATUS = Request.Params["PAYMENT_STATUS"];

            // Check PAYEE_ACCOUNT
            if (PAYEE_ACCOUNT != "Managerwar")
                return View();

            // Find batch number
            int iBatchNumber = Convert.ToInt32(BATCH_NUM);

            Deposit dep;
            ApplicationDbContext db = new ApplicationDbContext();
            // Find duplicate payments
            bool bDuplicate = false;            
            var DuplicateDeposit = db.Deposits.Where(q => q.BatchNo == iBatchNumber);
            if (DuplicateDeposit.Count() > 0)
            {
                bDuplicate = true;
                dep = DuplicateDeposit.First();
                // The deposit is duplicate return
                if (dep.Status != 0)
                    return null;
            }
            else
            {
                dep = new Deposit();
            }

            // check PAYER_ACCOUNT
            switch (PAYER_ACCOUNT)
            {
                case "Bitcoin":
                    dep.PaymentType = 2;
                    break;

                case "Litecoin":
                    dep.PaymentType = 3;
                    break;

                default:
                    dep.PaymentType = 1;
                    break;
            }

            // Find PAYMENT_AMOUNT
            decimal dAmount = Convert.ToDecimal(PAYMENT_AMOUNT);

            // Check PAYMENT_UNITS
            if (PAYMENT_UNITS != "USD")
                return View();

            // Find user
            var user = db.Users.Find(PAYMENT_ID);
            dep.User_Id = PAYMENT_ID;

            // Find fish
            if (CF_1 == "Squid")
                dep.Red = true;
            else if (CF_1 == "Bob")
                dep.Red = false;
            else
                throw new Exception("Fish not found");

            // Check round
            var round = db.Rounds.OrderByDescending(q => q.ID).First();
            if (!bDuplicate)
                dep.RoundID_ID = round.ID;
            dep.RoundID = round;

            // Status is 1 for now
            if (PAYMENT_STATUS == "Complete")
            {
                dep.Status = 1;
            }
            else
                dep.Status = 0;

            // Check MD5 hash
            string mypasshash = "4DE35585E43001E7436DE75DAE44B67F";
            string md5hash = PAYEE_ACCOUNT + "|" + PAYER_ACCOUNT + "|" + PAYMENT_AMOUNT + "|" + PAYMENT_UNITS + "|" + BATCH_NUM + "|" + PAYMENT_ID + "|" + CF_1 + "|" + CF_2 + "|" + mypasshash;
            md5hash = asmoneyapi.AsmoneyAPI.CalculateMD5Hash(md5hash);
            if (md5hash != MD5_HASH)
                throw new Exception("Hash dose not match");

            if (round.ID.ToString() != CF_2)
            {
                if (!bDuplicate || dep.PaymentType == 1)
                {
                    return null;
                }

                dep.RoundID_ID = Convert.ToInt64(CF_2);
                if (PAYMENT_STATUS == "Complete")
                {
                    dep.Status = 2;
                    // send money back to sender
                    asmoneyapi.AsmoneyAPI api = new asmoneyapi.AsmoneyAPI("managerwar", "api", "1532648sx");
                    user.Balance += dAmount;
                    int txid;
                    if (!string.IsNullOrEmpty(user.AsmoneyAccount))
                    {
                        api.Transfer(user.AsmoneyAccount, (double)dAmount, "USD", "Back to sender " + dAmount + " USD in round #" + dep.RoundID_ID, out txid);
                    }
                    else if (!string.IsNullOrEmpty(user.BitcoinAddress))
                    {
                        api.TransferBTC(user.BitcoinAddress, (double)dAmount, "USD", "Back to sender " + dAmount + " USD in round #" + dep.RoundID_ID, out txid);
                    }
                    else if (!string.IsNullOrEmpty(user.LitecoinAddress))
                    {
                        api.TransferLTC(user.LitecoinAddress, (double)dAmount, "USD", "Back to sender " + dAmount + " USD in round #" + dep.RoundID_ID, out txid);
                    }
                    dep.Amount = dAmount;
                    dep.Fee = 0;
                    dep.FeePercentage = 0;
                    dAmount = 0;
                }
            }
            else
            {
                if (!bDuplicate)
                {
                    // Calculate amount and fee
                    DateTime now = DateTime.UtcNow;
                    double feerate = now.Subtract(round.StartTime).TotalSeconds * 0.00002316;
                    dep.Amount = (decimal)(1 - feerate) * dAmount;
                    dep.Fee = (decimal)feerate * dAmount + (decimal)0.0001;
                    dep.FeePercentage = (decimal)feerate * 100;
                }
            }

            // If we recevied the money after round finished
            if (dep.FeePercentage >= 50)
            {
                dep.Status = 3;
                dep.Amount = 0;
                dep.Fee = dAmount;
                dep.FeePercentage = 100;
            }

            if (PAYMENT_STATUS == "Complete")
            {
                round.TotalDeposit += dAmount;
                round.TotalFee += dep.Fee;
                user.Balance -= dAmount;
            }
            
            // Add deposit to database
            if (!bDuplicate)
            {
                dep.BatchNo = iBatchNumber;
                db.Deposits.Add(dep);
            }

            db.SaveChanges();

            return View();

        } // asmoney

        [AllowAnonymous]
        public string PerfectMoney()
        {
            string PAYEE_ACCOUNT = Request.Params["PAYEE_ACCOUNT"];
            string PAYER_ACCOUNT = Request.Params["PAYER_ACCOUNT"];
            string PAYMENT_AMOUNT = Request.Params["PAYMENT_AMOUNT"];
            string PAYMENT_UNITS = Request.Params["PAYMENT_UNITS"];
            string BATCH_NUM = Request.Params["PAYMENT_BATCH_NUM"];
            string PAYMENT_ID = Request.Params["PAYMENT_ID"];
            string CF_1 = Request.Params["CF_1"];
            string CF_2 = Request.Params["CF_2"];
            string MD5_HASH = Request.Params["V2_HASH"];
            string TIMESTAMPGMT = Request.Params["TIMESTAMPGMT"];

            // Check PAYEE_ACCOUNT
            if (PAYEE_ACCOUNT != "U8289470")
                return "";

            // Find batch number
            int iBatchNumber = Convert.ToInt32(BATCH_NUM);
            
            ApplicationDbContext db = new ApplicationDbContext();
            // Find duplicate payments
            var DuplicateDeposit = db.Deposits.Where(q => q.BatchNo == iBatchNumber);
            if (DuplicateDeposit.Count() > 0)
                // The deposit is duplicate return
                return "";

            Deposit dep = new Deposit();
            dep.PaymentType = 4;
            dep.Status = 1;

            // Find PAYMENT_AMOUNT
            decimal dAmount = Convert.ToDecimal(PAYMENT_AMOUNT);

            // Check PAYMENT_UNITS
            if (PAYMENT_UNITS != "USD")
                return "";

            // Find user
            var user = db.Users.Find(PAYMENT_ID);
            dep.User_Id = PAYMENT_ID;

            // Find fish
            if (CF_1 == "Squid")
                dep.Red = true;
            else if (CF_1 == "Bob")
                dep.Red = false;
            else
                throw new Exception("Fish not found");

            // Check round
            var round = db.Rounds.OrderByDescending(q => q.ID).First();
            dep.RoundID_ID = round.ID;
            dep.RoundID = round;

            // Check MD5 hash
            string mypasshash = "4D4EF6BB8AEDCB9EE8194A993C06C484";
            string md5hash = PAYMENT_ID
                + ":" + PAYEE_ACCOUNT
                + ":" + PAYMENT_AMOUNT
                + ":" + PAYMENT_UNITS
                + ":" + BATCH_NUM
                + ":" + PAYER_ACCOUNT
                + ":" + mypasshash
                + ":" + TIMESTAMPGMT;
            md5hash = asmoneyapi.AsmoneyAPI.CalculateMD5Hash(md5hash);
            if (md5hash != MD5_HASH)
                throw new Exception("Hash dose not match");

            if (round.ID.ToString() != CF_2)
            {
                // If we recevied the money after round finished
                return null;
            }

            // Calculate amount and fee
            DateTime now = DateTime.UtcNow;
            double feerate = now.Subtract(round.StartTime).TotalSeconds * 0.00002316;
            dep.Amount = (decimal)(1 - feerate) * dAmount;
            dep.Fee = (decimal)feerate * dAmount + (decimal)0.0001;
            dep.FeePercentage = (decimal)feerate * 100;

            // If we recevied the money after round finished
            if (dep.FeePercentage >= 50)
            {
                dep.Status = 3;
                dep.Amount = 0;
                dep.Fee = dAmount;
                dep.FeePercentage = 100;
            }

            round.TotalDeposit += dAmount;
            round.TotalFee += dep.Fee;
            user.Balance -= dAmount;

            // Add deposit to database
            dep.BatchNo = iBatchNumber;
            db.Deposits.Add(dep);
            db.SaveChanges();

            return "";

        } // PerfectMoney


        //
        // POST: /Account/Disassociate
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Disassociate(string loginProvider, string providerKey)
        {
            ManageMessageId? message = null;
            IdentityResult result = await UserManager.RemoveLoginAsync(User.Identity.GetUserId(), new UserLoginInfo(loginProvider, providerKey));
            if (result.Succeeded)
            {
                message = ManageMessageId.RemoveLoginSuccess;
            }
            else
            {
                message = ManageMessageId.Error;
            }
            return RedirectToAction("Manage", new { Message = message });
        }

        MoneyViewModel GetMoneyModel()
        {
            MoneyViewModel m = new MoneyViewModel();

            var db = new ApplicationDbContext();
            var u = db.Users.Find(User.Identity.GetUserId());
            m.AsMoneyAccount = u.AsmoneyAccount;
            m.BitcoinAddress = u.BitcoinAddress;
            m.LitecoinAddress = u.LitecoinAddress;
            m.PerfectMoney = u.PerfectMoney;
            db.Dispose();

            return m;
        }

        //
        // GET: /Account/Manage
        public ActionResult Manage(ManageMessageId? message)
        {
            ViewBag.StatusMessage =
                message == ManageMessageId.ChangePasswordSuccess ? "Your password has been changed."
                : message == ManageMessageId.SetPasswordSuccess ? "Your password has been set."
                : message == ManageMessageId.RemoveLoginSuccess ? "The external login was removed."
                : message == ManageMessageId.Error ? "An error has occurred."
                : message == ManageMessageId.EditSuccess ? "Edit your account details success."
                : "";
            ViewBag.HasLocalPassword = HasPassword();
            ViewBag.ReturnUrl = Url.Action("Manage");
            ViewData["editmoney"] = GetMoneyModel();
            return View();
        }

        //
        // POST: /Account/Manage
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Manage(ManageUserViewModel model)
        {
            bool hasPassword = HasPassword();
            ViewBag.HasLocalPassword = hasPassword;
            ViewBag.ReturnUrl = Url.Action("Manage");
            if (hasPassword)
            {
                if (ModelState.IsValid)
                {
                    IdentityResult result = await UserManager.ChangePasswordAsync(User.Identity.GetUserId(), model.OldPassword, model.NewPassword);
                    if (result.Succeeded)
                    {
                        return RedirectToAction("Manage", new { Message = ManageMessageId.ChangePasswordSuccess });
                    }
                    else
                    {
                        AddErrors(result);
                    }
                }
            }
            else
            {
                // User does not have a password so remove any validation errors caused by a missing OldPassword field
                ModelState state = ModelState["OldPassword"];
                if (state != null)
                {
                    state.Errors.Clear();
                }

                if (ModelState.IsValid)
                {
                    IdentityResult result = await UserManager.AddPasswordAsync(User.Identity.GetUserId(), model.NewPassword);
                    if (result.Succeeded)
                    {
                        return RedirectToAction("Manage", new { Message = ManageMessageId.SetPasswordSuccess });
                    }
                    else
                    {
                        AddErrors(result);
                    }
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

         //
        // POST: /Account/Manage
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditMoney(MoneyViewModel model)
        {
            ViewBag.ReturnUrl = Url.Action("Manage");

            if (ModelState.IsValid)
            {
                var db = new ApplicationDbContext();
                var u = db.Users.Find(User.Identity.GetUserId());
                u.AsmoneyAccount = model.AsMoneyAccount;
                u.BitcoinAddress = model.BitcoinAddress;
                u.LitecoinAddress = model.LitecoinAddress;
                u.PerfectMoney = model.PerfectMoney;
                db.SaveChanges();
                db.Dispose();
                return RedirectToAction("Manage", new { Message = ManageMessageId.EditSuccess });
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // POST: /Account/ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            // Request a redirect to the external login provider
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        }

        //
        // GET: /Account/ExternalLoginCallback
        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                return RedirectToAction("Login");
            }

            // Sign in the user with this external login provider if the user already has a login
            var user = await UserManager.FindAsync(loginInfo.Login);
            if (user != null)
            {
                await SignInAsync(user, isPersistent: false);
                return RedirectToLocal(returnUrl);
            }
            else
            {
                // If the user does not have an account, then prompt the user to create an account
                ViewBag.ReturnUrl = returnUrl;
                ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
                return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = loginInfo.DefaultUserName });
            }
        }

        //
        // POST: /Account/LinkLogin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LinkLogin(string provider)
        {
            // Request a redirect to the external login provider to link a login for the current user
            return new ChallengeResult(provider, Url.Action("LinkLoginCallback", "Account"), User.Identity.GetUserId());
        }

        //
        // GET: /Account/LinkLoginCallback
        public async Task<ActionResult> LinkLoginCallback()
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync(XsrfKey, User.Identity.GetUserId());
            if (loginInfo == null)
            {
                return RedirectToAction("Manage", new { Message = ManageMessageId.Error });
            }
            var result = await UserManager.AddLoginAsync(User.Identity.GetUserId(), loginInfo.Login);
            if (result.Succeeded)
            {
                return RedirectToAction("Manage");
            }
            return RedirectToAction("Manage", new { Message = ManageMessageId.Error });
        }

        //
        // POST: /Account/ExternalLoginConfirmation
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Manage");
            }

            if (ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                var info = await AuthenticationManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return View("ExternalLoginFailure");
                }
                var user = new ApplicationUser() { UserName = model.Email };
                var result = await UserManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await UserManager.AddLoginAsync(user.Id, info.Login);
                    if (result.Succeeded)
                    {
                        await SignInAsync(user, isPersistent: false);
                        return RedirectToLocal(returnUrl);
                    }
                }
                AddErrors(result);
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        //
        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut();
            return RedirectToAction("Index", "Home");
        }

        //
        // GET: /Account/ExternalLoginFailure
        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        [ChildActionOnly]
        public ActionResult RemoveAccountList()
        {
            var linkedAccounts = UserManager.GetLogins(User.Identity.GetUserId());
            ViewBag.ShowRemoveButton = HasPassword() || linkedAccounts.Count > 1;
            return (ActionResult)PartialView("_RemoveAccountPartial", linkedAccounts);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && UserManager != null)
            {
                UserManager.Dispose();
                UserManager = null;
            }
            base.Dispose(disposing);
        }

        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private async Task SignInAsync(ApplicationUser user, bool isPersistent)
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ExternalCookie);
            var identity = await UserManager.CreateIdentityAsync(user, DefaultAuthenticationTypes.ApplicationCookie);
            AuthenticationManager.SignIn(new AuthenticationProperties() { IsPersistent = isPersistent }, identity);
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private bool HasPassword()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (user != null)
            {
                return user.PasswordHash != null;
            }
            return false;
        }

        public enum ManageMessageId
        {
            ChangePasswordSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess,
            EditSuccess,
            Error
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        private class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri) : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties() { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
        #endregion
    }
}