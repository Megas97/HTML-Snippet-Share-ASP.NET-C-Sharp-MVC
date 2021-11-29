using System;
using System.Linq;
using System.Web.Mvc;
using System.IO;
using HTMLSnippetShare.Models;
using System.Text.RegularExpressions;
using System.Web;

namespace HTMLSnippetShare.Controllers
{
    public class HomeController : Controller
    {
        #region // HTML Snippet Page
        [HttpGet]
        public ActionResult Index(int id = 0)
        {
            if (Request.Url.Segments.Length > 1)
            {
                if (!Regex.IsMatch(Request.Url.Segments[1], @"^\d+$"))
                {
                    throw new HttpException(404, "Page Not Found");
                }
            }
            if (id == 0)
            {
                ViewBag.HTMLCodeInput = TempData["HTMLCodeInput"];
                ViewBag.HTMLCodePreview = TempData["HTMLCodePreview"];
                ViewBag.Message = TempData["Message"];
            }
            else
            {
                if (id < 0)
                {
                    ViewBag.Message = "Please enter a positive integer value";
                }
                else
                {
                    using (DatabaseEntities db = new DatabaseEntities())
                    {
                        HTMLCode code = db.HTMLCodes.Where(a => a.Id == id).FirstOrDefault();
                        if (code != null)
                        {
                            ViewBag.HTMLCodeID = code.Id;
                            ViewBag.HTMLCodeInput = TempData["HTMLCodeInput"] == null ? Base64Decode(code.HTML) : TempData["HTMLCodeInput"];
                            ViewBag.HTMLCodePreview = TempData["HTMLCodePreview"] == null ? Base64Decode(code.HTML) : TempData["HTMLCodePreview"];
                            ViewBag.CreatedOn = code.Created;
                            ViewBag.LastModified = code.Edited;
                            ViewBag.EditMessage = "Showing result for id " + id;
                        }
                        else
                        {
                            ViewBag.EditMessage = "No result found for id " + id;
                        }
                        ViewBag.Message = TempData["Message"];
                    }
                }
            }
            return View();
        }
        #endregion

        #region // HTML Snippet Actions
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult ProcessFormSubmit(string previewButton, string checkButton, string saveButton, FormCollection formCollection)
        {
            int HTMLCodeID = 0;
            if (formCollection["HTMLCodeInput"] == "")
            {
                TempData["Message"] = "Please write some HTML code first";
                HTMLCodeID = formCollection["HTMLCodeID"] == "" ? 0 : Convert.ToInt32(formCollection["HTMLCodeID"]);
                TempData["HTMLCodeInput"] = formCollection["HTMLCodeInput"];
                TempData["HTMLCodePreview"] = formCollection["HTMLCodeInput"];
            }
            else
            {
                if (!String.IsNullOrEmpty(previewButton))
                {
                    HTMLCodeID = formCollection["HTMLCodeID"] == "" ? 0 : Convert.ToInt32(formCollection["HTMLCodeID"]);
                    TempData["HTMLCodeInput"] = formCollection["HTMLCodeInput"];
                    TempData["HTMLCodePreview"] = formCollection["HTMLCodeInput"];
                }
                else if (!String.IsNullOrEmpty(checkButton))
                {
                    using (DatabaseEntities db = new DatabaseEntities())
                    {
                        string HTMLInput = Base64Encode(formCollection["HTMLCodeInput"]);
                        HTMLCode code = db.HTMLCodes.Where(a => a.HTML.Equals(HTMLInput)).FirstOrDefault();
                        int CodeID = formCollection["HTMLCodeID"] == "" ? 0 : Convert.ToInt32(formCollection["HTMLCodeID"]);
                        HTMLCode existingHTMLCode = db.HTMLCodes.Where(a => a.Id == CodeID).FirstOrDefault();
                        User currentUser = db.Users.Where(a => a.Username.Equals(HttpContext.User.Identity.Name)).FirstOrDefault();
                        string text = "your";
                        if (existingHTMLCode != null && currentUser != null)
                        {
                            text = existingHTMLCode.UserId == currentUser.Id ? "your" : "the";
                        }
                        TempData["Message"] = code != null ? "An identical HTML code already exists in the database" : System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(text) + " HTML code is unique";
                    }
                    HTMLCodeID = formCollection["HTMLCodeID"] == "" ? 0 : Convert.ToInt32(formCollection["HTMLCodeID"]);
                    TempData["HTMLCodeInput"] = formCollection["HTMLCodeInput"];
                    TempData["HTMLCodePreview"] = formCollection["HTMLCodeInput"];
                }
                else if (!String.IsNullOrEmpty(saveButton))
                {
                    if (!Request.IsAuthenticated)
                    {
                        HTMLCodeID = formCollection["HTMLCodeID"] == "" ? 0 : Convert.ToInt32(formCollection["HTMLCodeID"]);
                        TempData["HTMLCodeInput"] = formCollection["HTMLCodeInput"];
                        TempData["HTMLCodePreview"] = formCollection["HTMLCodeInput"];
                        TempData["Message"] = "Only logged in users can save HTML code";
                    }
                    else
                    {
                        var tempFilePath = Path.GetTempFileName();
                        using (StreamWriter outputFile = new StreamWriter(tempFilePath))
                        {
                            outputFile.WriteLine(formCollection["HTMLCodeInput"]);
                        }
                        int tempFileSizeLimit = 5242880; // 5 MB
                        long tempFileSize = 0;
                        using (StreamReader inputFile = new StreamReader(tempFilePath))
                        {
                            tempFileSize = inputFile.BaseStream.Length;
                        }
                        var file = new FileInfo(tempFilePath);
                        file.Delete();
                        if (tempFileSize < tempFileSizeLimit)
                        {
                            using (DatabaseEntities db = new DatabaseEntities())
                            {
                                int CodeID = formCollection["HTMLCodeID"] == "" ? 0 : Convert.ToInt32(formCollection["HTMLCodeID"]);
                                HTMLCode existingHTMLCode = db.HTMLCodes.Where(a => a.Id == CodeID).FirstOrDefault();
                                User currentUser = db.Users.Where(a => a.Username.Equals(HttpContext.User.Identity.Name)).FirstOrDefault();
                                if (existingHTMLCode != null && currentUser != null)
                                {
                                    if ((existingHTMLCode.UserId != currentUser.Id) && (currentUser.IsAdmin == false))
                                    {
                                        HTMLCodeID = existingHTMLCode.Id;
                                        TempData["HTMLCodeInput"] = formCollection["HTMLCodeInput"];
                                        TempData["HTMLCodePreview"] = formCollection["HTMLCodeInput"];
                                        TempData["Message"] = "Only the owner of this snippet can edit it";
                                    }
                                    else
                                    {
                                        string text = existingHTMLCode.UserId == currentUser.Id ? "your" : "the";
                                        if (Base64Encode(formCollection["HTMLCodeInput"]).Equals(existingHTMLCode.HTML))
                                        {
                                            TempData["Message"] = "You did not make any changes to " + text + " HTML code";
                                        }
                                        else
                                        {
                                            existingHTMLCode.HTML = Base64Encode(formCollection["HTMLCodeInput"]);
                                            existingHTMLCode.Edited = DateTime.Now;
                                            db.Configuration.ValidateOnSaveEnabled = false;
                                            db.SaveChanges();
                                            TempData["Message"] = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(text) + " HTML code was successfully updated";
                                        }
                                        HTMLCodeID = existingHTMLCode.Id;
                                    }
                                }
                                else
                                {
                                    HTMLCode code = new HTMLCode();
                                    code.HTML = Base64Encode(formCollection["HTMLCodeInput"]);
                                    code.Created = DateTime.Now;
                                    code.Edited = null;
                                    code.User = currentUser;
                                    db.HTMLCodes.Add(code);
                                    db.SaveChanges();
                                    TempData["Message"] = "Your HTML code was successfully saved";
                                    HTMLCodeID = code.Id;
                                }
                            }
                        }
                        else
                        {
                            TempData["Message"] = "The maximum allowed size of the HTML code is 5 MB";
                            HTMLCodeID = formCollection["HTMLCodeID"] == "" ? 0 : Convert.ToInt32(formCollection["HTMLCodeID"]);
                        }
                    }
                }
            }
            return HTMLCodeID != 0 ? RedirectToAction("Index", new { id = HTMLCodeID }) : RedirectToAction("Index");
        }
        #endregion

        #region // My Snippets Page
        [HttpGet]
        [Authorize]
        public ActionResult MySnippets()
        {
            using (DatabaseEntities db = new DatabaseEntities())
            {
                User user = db.Users.Where(a => a.Username.Equals(HttpContext.User.Identity.Name)).FirstOrDefault();
                var codesList = db.HTMLCodes.Where(a => a.UserId == user.Id);
                codesList.ToList().ForEach(a => a.HTML = Base64Decode(a.HTML));
                ViewBag.DeleteSnippetMessage = TempData["DeleteSnippetMessage"] == null ? "" : TempData["DeleteSnippetMessage"];
                return View(codesList.ToList().OrderByDescending(a => a.Id));
            }
        }
        #endregion

        #region // Delete Snippet Action
        [HttpGet]
        [Authorize]
        public ActionResult DeleteSnippet(int id)
        {
            using (DatabaseEntities db = new DatabaseEntities())
            {
                if (id <= 0)
                {
                    TempData["DeleteSnippetMessage"] = "Please enter a positive integer value";
                }
                else
                {
                    User user = db.Users.Where(a => a.Username.Equals(HttpContext.User.Identity.Name)).FirstOrDefault();
                    HTMLCode code = db.HTMLCodes.Where(a => a.Id == id).FirstOrDefault();
                    if (code == null)
                    {
                        TempData["DeleteSnippetMessage"] = "There is no snippet with id " + id;
                    }
                    else
                    {
                        if (code.UserId != user.Id)
                        {
                            TempData["DeleteSnippetMessage"] = "Only the owner of this snippet can delete it";
                        }
                        else
                        {
                            db.HTMLCodes.Remove(code);
                            db.SaveChanges();
                            TempData["DeleteSnippetMessage"] = "Snippet with id " + id + " successfully deleted";
                        }
                    }
                }
            }
            return RedirectToAction("MySnippets");

        }
        #endregion

        #region // Not Found Page
        [HttpGet]
        public ActionResult NotFound()
        {
            return View();
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