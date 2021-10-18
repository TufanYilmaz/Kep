using KepNotificationDev.Helpers;
using KepNotificationDev.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;

namespace KepNotificationDev.Controllers
{
    [AllowAnonymous]
    public class AuthController : Controller
    {
        public ActionResult Index()
        {
            return RedirectToAction("Login");
        }

        // GET: Auth
        public ActionResult Login()
        {
            var model = new User();
            return View(model);
        }
        [HttpPost]
        public ActionResult Login(User user)
        {
            Authentication.AuthenticationSoapClient client = new Authentication.AuthenticationSoapClient();

            if (!ModelState.IsValid)
            {
                return View();
            }
            var res = client.Login(user.Username, user.Password, "");
            if (res.Result.Id == 0)
            {
                FormsAuthentication.SetAuthCookie(user.Username, false);
                Session["Username"] = user.Username;
                Session["SessionId"] = res.Value;
                Session["Userinfo"] = res.User.Info;
                //Session["UserRole"] =res.User.

                Helpers.Session.Username = user.Username;
                Helpers.Session.UserId = res.User.Id;
                Helpers.Session.SessionId = res.Value;
                return RedirectToAction("Index", "Home");
            }
            return View();

        }
        public ActionResult LogOut()
        {
            FormsAuthentication.SignOut();
            Session["Username"] = "";
            Session["SessionId"] = "";
            Session["Userinfo"] = "";

            Helpers.Session.Username = "";
            Helpers.Session.UserId = -1;
            Helpers.Session.SessionId = Guid.Empty;
            return RedirectToAction("Login", "Auth");
        }
    }
}