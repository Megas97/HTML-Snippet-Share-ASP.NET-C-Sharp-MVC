using HTMLSnippetShare.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HTMLSnippetShare.Controllers
{
    public class AdminController : Controller
    {
        #region // Admin Panel Page
        [HttpGet]
        [Authorize]
        public ActionResult AdminPanel()
        {
            using (DatabaseEntities db = new DatabaseEntities())
            {
                User user = db.Users.Where(a => a.Username.Equals(HttpContext.User.Identity.Name)).FirstOrDefault();
                if (user != null)
                {
                    if (user.IsAdmin == true)
                    {
                        ViewBag.ChangeUserStatusMessage = TempData["ChangeUserStatusMessage"] == null ? "" : TempData["ChangeUserStatusMessage"];
                        ViewBag.DeleteUserMessage = TempData["DeleteUserMessage"] == null ? "" : TempData["DeleteUserMessage"];
                        return View();
                    }
                }
                TempData["Message"] = "Only admins can access the admin panel";
                return RedirectToAction("Index", "Home");
            }
        }
        #endregion

        #region // Change User Status
        [HttpPost]
        [ValidateInput(false)]
        [Authorize]
        public ActionResult ChangeUserStatus(string grantAdminButton, string revokeAdminButton, FormCollection formCollection)
        {
            if (formCollection["UsersList"] != "")
            {
                using (DatabaseEntities db = new DatabaseEntities())
                {
                    string selectedUser = formCollection["UsersList"].ToString();
                    User user = db.Users.Where(a => a.Username.Equals(selectedUser)).FirstOrDefault();
                    if (user != null)
                    {
                        if (!String.IsNullOrEmpty(grantAdminButton))
                        {
                            if (user.IsAdmin == true)
                            {
                                TempData["ChangeUserStatusMessage"] = user.Username + " already has admin rights";
                            }
                            else
                            {
                                user.IsAdmin = true;
                                db.Configuration.ValidateOnSaveEnabled = false;
                                db.SaveChanges();
                                TempData["ChangeUserStatusMessage"] = user.Username + " was granted admin rights";
                            }
                        }
                        else if (!String.IsNullOrEmpty(revokeAdminButton))
                        {
                            if (user.IsAdmin == false)
                            {
                                TempData["ChangeUserStatusMessage"] = user.Username + " does not have admin rights";
                            }
                            else
                            {
                                user.IsAdmin = false;
                                db.Configuration.ValidateOnSaveEnabled = false;
                                db.SaveChanges();
                                TempData["ChangeUserStatusMessage"] = user.Username + "'s admin rights were revoked";
                            }
                        }
                    }
                    else
                    {
                        TempData["ChangeUserStatusMessage"] = "No such user exists";
                    }
                }
            }
            else
            {
                TempData["ChangeUserStatusMessage"] = "Please select a user to which to grant or from which to revoke admin rights";
            }
            return RedirectToAction("AdminPanel");
        }
        #endregion

        #region // Delete User Action
        [HttpPost]
        [Authorize]
        public ActionResult DeleteUser(FormCollection formCollection)
        {
            if (formCollection["UsersList2"] != "")
            {
                using (DatabaseEntities db = new DatabaseEntities())
                {
                    string selectedUser = formCollection["UsersList2"].ToString();
                    User user = db.Users.Where(a => a.Username.Equals(selectedUser)).FirstOrDefault();
                    if (user != null)
                    {
                        string codesText = "";
                        var codes = db.HTMLCodes.Where(a => a.UserId == user.Id);
                        foreach (var code in codes)
                        {
                            if (code != null)
                            {
                                db.HTMLCodes.Remove(code);
                                codesText = "and all their HTML snippets ";
                            }
                        }
                        db.Users.Remove(user);
                        db.SaveChanges();
                        TempData["DeleteUserMessage"] = "User '" + user.Username + "' " + codesText + "successfully deleted";
                    }
                    else
                    {
                        TempData["DeleteUserMessage"] = "No such user exists";
                    }
                }
            }
            else
            {
                TempData["DeleteUserMessage"] = "Please select a user to delete";
            }
            return RedirectToAction("AdminPanel");
        }
        #endregion

        #region // User Snippets Page
        [HttpGet]
        [Authorize]
        public ActionResult UserSnippets(int? id = null)
        {
            if (id == null)
            {
                using (DatabaseEntities db = new DatabaseEntities())
                {
                    User currentUser = db.Users.Where(a => a.Username.Equals(HttpContext.User.Identity.Name)).FirstOrDefault();
                    if (currentUser != null)
                    {
                        if (currentUser.IsAdmin == false)
                        {
                            throw new HttpException(404, "Page Not Found");
                        }
                    }
                    ViewBag.DeleteUserSnippetsMessage = "Please enter a user id";
                }
            }
            else
            {
                if (id <= 0)
                {
                    ViewBag.DeleteUserSnippetsMessage = "Please enter a positive integer value";
                }
                else
                {
                    using (DatabaseEntities db = new DatabaseEntities())
                    {
                        User currentUser = db.Users.Where(a => a.Username.Equals(HttpContext.User.Identity.Name)).FirstOrDefault();
                        if (currentUser != null)
                        {
                            if (currentUser.IsAdmin == true)
                            {
                                User user = db.Users.Where(a => a.Id == id).FirstOrDefault();
                                if (user != null)
                                {
                                    var codesList = db.HTMLCodes.Where(a => a.UserId == user.Id);
                                    ViewBag.Username = user.Username;
                                    codesList.ToList().ForEach(a => a.HTML = Base64Decode(a.HTML));
                                    return View(codesList.ToList().OrderByDescending(a => a.Id));
                                }
                                else
                                {
                                    ViewBag.DeleteUserSnippetsMessage = "No such user exists";
                                }
                            }
                            else
                            {
                                throw new HttpException(404, "Page Not Found");
                            }
                        }
                    }
                }
            }
            ViewBag.DeleteUserSnippetsMessage = TempData["DeleteUserSnippetMessage"] != null ? TempData["DeleteUserSnippetMessage"] : ViewBag.DeleteUserSnippetsMessage;
            ViewBag.Username = "Nobody";
            return View(new List<HTMLCode>());
        }
        #endregion

        #region // User Snippets Action
        [HttpPost]
        [Authorize]
        public ActionResult UserSnippets(FormCollection formCollection)
        {
            return RedirectToAction("UserSnippets", "Admin", new { id = Convert.ToInt32(formCollection["UsersList3"]) });
        }
        #endregion

        #region // Delete User Snippet Action
        [HttpGet]
        [Authorize]
        public ActionResult DeleteUserSnippet(int id, string username = "")
        {
            using (DatabaseEntities db = new DatabaseEntities())
            {
                if (id <= 0)
                {
                    TempData["DeleteUserSnippetMessage"] = "Please enter a positive integer value";
                }
                else
                {
                    User currentUser = db.Users.Where(a => a.Username.Equals(HttpContext.User.Identity.Name)).FirstOrDefault();
                    if (currentUser != null)
                    {
                        if (currentUser.IsAdmin == true)
                        {
                            HTMLCode code = db.HTMLCodes.Where(a => a.Id == id).FirstOrDefault();
                            if (code == null)
                            {
                                TempData["DeleteUserSnippetMessage"] = "There is no snippet with id " + id;
                            }
                            else
                            {
                                db.HTMLCodes.Remove(code);
                                db.SaveChanges();
                                TempData["DeleteUserSnippetMessage"] = "Snippet with id " + id + " successfully deleted";
                            }
                        }
                    }
                }
                User user = db.Users.Where(a => a.Username.Equals(username)).FirstOrDefault();
                int? userId = user?.Id;
                return RedirectToAction("UserSnippets", "Admin", new { id = userId });
            }
        }
        #endregion

        #region // Helper Functions
        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        public static string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }
        #endregion
    }
}