using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using HTMLSnippetShare.Models;

namespace HTMLSnippetShare.Controllers
{
    public class UserController : Controller
    {
        readonly DatabaseEntities db = new DatabaseEntities();

        #region // Register Page
        [HttpGet]
        public ActionResult Register()
        {
            return View();
        }
        #endregion

        #region // Register Action
        [HttpPost]
        public ActionResult Register(User user)
        {
            if (db.Users.Any(a => a.Username.Equals(user.Username)))
            {
                ViewBag.Notification = "This username is already taken";
                return View();
            }
            else
            {
                user.Password = Crypto.Hash(user.Password);
                user.ConfirmPassword = Crypto.Hash(user.Password);
                user.IsAdmin = db.Users.Count() < 1 ? true : false;
                db.Users.Add(user);
                db.Configuration.ValidateOnSaveEnabled = false;
                db.SaveChanges();
                ViewBag.Notification = "You registered successfully";
                return View(user);
            }
        }
        #endregion

        #region // Login Page
        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }
        #endregion

        #region // Login Action
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(User user)
        {
            string hashedPassword = Crypto.Hash(user.Password);
            var check = db.Users.Where(a => a.Username.Equals(user.Username) && a.Password.Equals(hashedPassword)).FirstOrDefault();
            if (check != null)
            {
                int timeout = user.RememberMe ? 525600 : 20; // 525600 mins = 1 year
                var ticket = new FormsAuthenticationTicket(user.Username, user.RememberMe, timeout);
                string encrypted = FormsAuthentication.Encrypt(ticket);
                var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encrypted);
                cookie.Expires = DateTime.Now.AddMinutes(timeout);
                cookie.HttpOnly = true;
                Response.Cookies.Add(cookie);
                return RedirectToAction("Index", "Home");
            }
            else
            {
                ViewBag.Notification = "Wrong username or password";
            }
            return View();
        }
        #endregion

        #region // Logout Action
        [HttpGet]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Index", "Home");
        }
        #endregion
    }
}