using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using DotNetOpenAuth.AspNet;
using Microsoft.Web.WebPages.OAuth;
using WebMatrix.WebData;
using PPlanner.Models;
using System.Data.SqlClient;
using System.Data.Entity;
using System.Drawing;
using System.Web.Routing;
using MvcSiteMapProvider;
using System.IO;
using System.Drawing.Imaging;

namespace PPlanner.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        ProjectDbContext db = new ProjectDbContext();
        //
        // GET: /Account/Login
        //override unauthorizedrequest in projects
        public class MyAuthorizeAttribute : AuthorizeAttribute
        {
            protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
            {
                filterContext.Result = new RedirectToRouteResult(
                                   new RouteValueDictionary
                                   {
                                       { "action", "Error" },
                                       { "controller", "Shared"} 
                                   });
            }
        }

        public void GetProjectNameForBreadcrumbs(UserProfile userprofile)
        {
            var node = SiteMaps.Current.CurrentNode;

            if (node != null && node.ParentNode.Title != null)
            {
                node.ParentNode.Title = userprofile.UserName;

            }
        }

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
        public ActionResult Login(LoginModel model, string returnUrl)
        {
             
            if (ModelState.IsValid)
            {
                if (WebSecurity.UserExists(model.UserName))
                {                    
                    if (db.UserProfiles.Where(up => up.UserName == model.UserName).Select(up => up.isApproved).FirstOrDefault() || 
                        Roles.IsUserInRole(model.UserName, "Admin"))
                    {
                        if (WebSecurity.Login(model.UserName, model.Password, persistCookie: model.RememberMe))
                        {
                            if (String.IsNullOrEmpty(returnUrl))
                            {
                                return RedirectToAction("Index", "Project");
                            }

                            return RedirectToLocal(returnUrl);
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("", "Вашият потребител не е потвърден от администратор.");
                        return View(model);
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Избрали сте несъществуващо потребителско име");
                    return View(model);
                }
            }

            // If we got this far, something failed, redisplay form
            ModelState.AddModelError("", "Въвели сте грешно потребителско име и/или парола.");
            return View(model);
        }

        //
        // POST: /Account/LogOff

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
             
            WebSecurity.Logout();

            return RedirectToAction("Index", "Project");
        }

        //
        // GET: /Account/Register

        [AllowAnonymous]
        public ActionResult Register()
        {
             
            return View();
        }

        public ActionResult UploadImage(string name)
        {
            UserProfile up = db.UserProfiles.Where(u => u.UserName == name).FirstOrDefault();
            return View(up);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UploadImage(HttpPostedFileBase fileUpload, UserProfile user)
        {
            if (ModelState.IsValid)
            {
                if (fileUpload != null)
                {
                    byte[] uImage = new byte[fileUpload.ContentLength];
                    using (var binaryR = new BinaryReader(fileUpload.InputStream))
                    {
                        uImage = binaryR.ReadBytes(fileUpload.ContentLength);
                    };
                    user = db.UserProfiles.Where(u => u.UserName == user.UserName).FirstOrDefault();
                    user.Image = uImage;

                    db.Entry(user).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }
            return RedirectToAction("Details", "Account", new { name = user.UserName });
        }

        //DEtails
        public ActionResult Details(string name)
        {
             
            if (name != null)
            {
                UserProfile usr = db.UserProfiles.Find(WebSecurity.GetUserId(name));
                ViewBag.UserName = usr.UserName;
                ViewBag.EmailAddress = usr.Email;
                ViewBag.Name = usr.FirstName + " " + usr.LastName;
                ViewBag.Position = usr.Position;
                ViewBag.UserId = usr.UserId;
                ViewBag.isApproved = usr.isApproved;

                if (Roles.GetRolesForUser().Length != 0)
                {
                    ViewBag.UserRole = Roles.GetRolesForUser()[0].ToString();
                }

                List<UserStory> userstories = db.UserStories
                        .Where(us => us.UserProfile_UserId == usr.UserId)
                        .ToList();

                ViewData["userstories"] = userstories;
                GetProjectNameForBreadcrumbs(usr);
                if (usr.Image != null)
                {                
                    using (var ms = new MemoryStream(usr.Image))
                    {
                        var img = String.Format("data:image/jpg;base64,{0}", Convert.ToBase64String(usr.Image));
                        ViewBag.Image = img;
                    }
                }

                return View(userstories);
            }
            else
            {
                if (User.Identity.Name != null)
                {
                    UserProfile usr = db.UserProfiles.Find(WebSecurity.GetUserId(User.Identity.Name));
                    ViewBag.UserName = usr.UserName;
                    ViewBag.EmailAddress = usr.Email;
                    ViewBag.Name = usr.FirstName + " " + usr.LastName;
                    ViewBag.Position = usr.Position;
                    ViewBag.UserId = usr.UserId;
                    ViewBag.isApproved = usr.isApproved;

                    if (Roles.GetRolesForUser().Length != 0)
                    {
                        ViewBag.UserRole = Roles.GetRolesForUser()[0].ToString();
                    }

                    List<UserStory> userstories = db.UserStories
                            .Where(us => us.UserProfile_UserId == usr.UserId)
                            .ToList();

                    ViewData["userstories"] = userstories;

                    GetProjectNameForBreadcrumbs(usr);
                    if (usr.Image != null)
                    {
                        using (var ms = new MemoryStream(usr.Image))
                        {
                            var img = String.Format("data:image/jpg;base64,{0}", Convert.ToBase64String(usr.Image));
                            ViewBag.Image = img;
                        }
                    }
                    
                    return View(userstories);
                }
                else
                {
                    return View();
                }
            }
        }

        //[MyAuthorize(Roles = "Admin")]
        public ActionResult Edit(string name)
        {
             
            UserProfile userprofile = db.UserProfiles.Find(WebSecurity.GetUserId(name));
            if (userprofile == null)
            {
                return HttpNotFound();
            }
            IEnumerable<SelectListItem> RolesList = new System.Web.Mvc.SelectList(System.Web.Security.Roles.GetAllRoles(), "RoleName");
            ViewData["RoleName"] = RolesList;
            GetProjectNameForBreadcrumbs(userprofile);
            return View(userprofile);
        }

        [HttpPost]
        public ActionResult Edit(UserProfile userprofile)
        {
             
            if (ModelState.IsValid)
            {
                if (Roles.GetRolesForUser(userprofile.UserName).Length != 0)
                {
                    Roles.RemoveUserFromRole(userprofile.UserName, Roles.GetRolesForUser(userprofile.UserName)[0].ToString());
                }

                if (userprofile.RoleName != null)
                {
                    Roles.AddUserToRole(userprofile.UserName, userprofile.RoleName);
                }
                
                db.Entry(userprofile).State = EntityState.Modified;
                db.SaveChanges();
                GetProjectNameForBreadcrumbs(userprofile);

                return RedirectToAction("Details", "Account", new { name=userprofile.UserName});
            }
            GetProjectNameForBreadcrumbs(userprofile);

            return RedirectToAction("Index", "Project");
            
        }



        //
        // POST: /Account/Register

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterModel model)
        {
             
            if (ModelState.IsValid)
            {
                // Attempt to register the user
                try
                {
                    WebSecurity.CreateUserAndAccount(model.UserName, model.Password, new { isApproved = false }, true);
                    
                    //WebSecurity.Login(model.UserName, model.Password);

                    return RedirectToAction("Index", "Project");
                }
                catch (MembershipCreateUserException e)
                {
                    ModelState.AddModelError("", ErrorCodeToString(e.StatusCode));
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // POST: /Account/Disassociate

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Disassociate(string provider, string providerUserId)
        {
             
            string ownerAccount = OAuthWebSecurity.GetUserName(provider, providerUserId);
            ManageMessageId? message = null;

            // Only disassociate the account if the currently logged in user is the owner
            if (ownerAccount == User.Identity.Name)
            {
                // Use a transaction to prevent the user from deleting their last login credential
                using (var scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Serializable }))
                {
                    bool hasLocalAccount = OAuthWebSecurity.HasLocalAccount(WebSecurity.GetUserId(User.Identity.Name));
                    if (hasLocalAccount || OAuthWebSecurity.GetAccountsFromUserName(User.Identity.Name).Count > 1)
                    {
                        OAuthWebSecurity.DeleteAccount(provider, providerUserId);
                        scope.Complete();
                        message = ManageMessageId.RemoveLoginSuccess;
                    }
                }
            }

            return RedirectToAction("Manage", new { Message = message });
        }

        //
        // GET: /Account/Manage

        public ActionResult Manage(ManageMessageId? message)
        {
             
            ViewBag.StatusMessage =
                message == ManageMessageId.ChangePasswordSuccess ? "Вашата парола е променена."
                : message == ManageMessageId.SetPasswordSuccess ? "Вашата парола е запаметена"
                : message == ManageMessageId.RemoveLoginSuccess ? "Външният вход е премахнат"
                : "";
            ViewBag.HasLocalPassword = OAuthWebSecurity.HasLocalAccount(WebSecurity.GetUserId(User.Identity.Name));
            ViewBag.ReturnUrl = Url.Action("Manage");
            return View();
        }

        //
        // POST: /Account/Manage

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Manage(LocalPasswordModel model)
        {
             
            bool hasLocalAccount = OAuthWebSecurity.HasLocalAccount(WebSecurity.GetUserId(User.Identity.Name));
            ViewBag.HasLocalPassword = hasLocalAccount;
            ViewBag.ReturnUrl = Url.Action("Manage");
            if (hasLocalAccount)
            {
                if (ModelState.IsValid)
                {
                    // ChangePassword will throw an exception rather than return false in certain failure scenarios.
                    bool changePasswordSucceeded;
                    try
                    {
                        changePasswordSucceeded = WebSecurity.ChangePassword(User.Identity.Name, model.OldPassword, model.NewPassword);
                    }
                    catch (Exception)
                    {
                        changePasswordSucceeded = false;
                    }

                    if (changePasswordSucceeded)
                    {
                        return RedirectToAction("Manage", new { Message = ManageMessageId.ChangePasswordSuccess });
                    }
                    else
                    {
                        ModelState.AddModelError("", "Сегашната парола е грешна или новата е невалидна.");
                    }
                }
            }
            else
            {
                // User does not have a local password so remove any validation errors caused by a missing
                // OldPassword field
                ModelState state = ModelState["OldPassword"];
                if (state != null)
                {
                    state.Errors.Clear();
                }

                if (ModelState.IsValid)
                {
                    try
                    {
                        WebSecurity.CreateAccount(User.Identity.Name, model.NewPassword);
                        return RedirectToAction("Manage", new { Message = ManageMessageId.SetPasswordSuccess });
                    }
                    catch (Exception e)
                    {
                        ModelState.AddModelError("", e);
                    }
                }
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

            return new ExternalLoginResult(provider, Url.Action("ExternalLoginCallback", new { ReturnUrl = returnUrl }));
        }

        //
        // GET: /Account/ExternalLoginCallback

        [AllowAnonymous]
        public ActionResult ExternalLoginCallback(string returnUrl)
        {
            AuthenticationResult result = OAuthWebSecurity.VerifyAuthentication(Url.Action("ExternalLoginCallback", new { ReturnUrl = returnUrl }));
            if (!result.IsSuccessful)
            {
                return RedirectToAction("ExternalLoginFailure");
            }

            if (OAuthWebSecurity.Login(result.Provider, result.ProviderUserId, createPersistentCookie: false))
            {
                return RedirectToLocal(returnUrl);
            }

            if (User.Identity.IsAuthenticated)
            {
                // If the current user is logged in add the new account
                OAuthWebSecurity.CreateOrUpdateAccount(result.Provider, result.ProviderUserId, User.Identity.Name);
                return RedirectToLocal(returnUrl);
            }
            else
            {
                // User is new, ask for their desired membership name
                string loginData = OAuthWebSecurity.SerializeProviderUserId(result.Provider, result.ProviderUserId);
                ViewBag.ProviderDisplayName = OAuthWebSecurity.GetOAuthClientData(result.Provider).DisplayName;
                ViewBag.ReturnUrl = returnUrl;
                return View("ExternalLoginConfirmation", new RegisterExternalLoginModel { UserName = result.UserName, ExternalLoginData = loginData });
            }
        }

        [MyAuthorize(Roles="Admin")]
        public ActionResult UserManage()
        {
             
            List<UserProfile> users = db.UserProfiles
                                        .Where(u => u.UserName != "admin")
                                        .ToList();
            UserProfile userprofile = new UserProfile();

            GetProjectNameForBreadcrumbs(userprofile);
            return View(users);
        }

        [MyAuthorize(Roles = "Admin")]
        public ActionResult CreateRole()
        {
             
            return View();
        }

        [MyAuthorize(Roles = "Admin")]
        public ActionResult UserRoles()
        {
             
            return View();
        }

        [MyAuthorize(Roles = "Admin")]
        public ActionResult ApproveUser(string UserName,bool Approve)
        {
             
            if (WebSecurity.UserExists(UserName))
            {
                string constring = System.Configuration.ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

                SqlConnection sqlConnection = new SqlConnection(constring);
                SqlCommand sqlCommand = new SqlCommand();
                sqlCommand.Connection = sqlConnection;
                sqlCommand.CommandText = 
                "UPDATE webpages_Membership " +
                " SET IsConfirmed=" + (Approve?1:0) +
                " WHERE UserId= " + WebSecurity.GetUserId(UserName);

                try
                {
                    sqlConnection.Open();
                    sqlCommand.ExecuteNonQuery();

                    UserProfile usr =  db.UserProfiles.Find(WebSecurity.GetUserId(UserName));
                    usr.isApproved = Approve;
                    db.Entry(usr).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();

                }
                catch (Exception ex)
                {
                }
                finally
                {
                    if (sqlConnection.State == System.Data.ConnectionState.Open)
                        sqlConnection.Close();
                }

                return RedirectToAction("UserManage");
            }

            return HttpNotFound();
        }

        [MyAuthorize(Roles = "Admin")]
        public ActionResult DeleteUser(string UserName)
        {
             
            UserProfile user = db.UserProfiles.Find(WebSecurity.GetUserId(UserName));

            //if (WebSecurity.UserExists(UserName))
            if (user != null)
            {       
                List<UserStory> userstories = db.UserStories.Where(us => us.UserProfile_UserId == user.UserId).ToList();
               // userstories.ForEach(us => us.UserProfile_UserId = null);
                foreach (UserStory us in userstories)
                {
                    us.UserProfile_UserId = null;
                    db.Entry(us).State = System.Data.Entity.EntityState.Modified;
                }

                db.UserProfiles.Remove(user);

                //db.Entry(userstories).State = System.Data.EntityState.Modified;

                db.SaveChanges();

                string constring = System.Configuration.ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
                SqlConnection sqlConnection = new SqlConnection(constring);
                SqlCommand sqlCommand = new SqlCommand();
                sqlCommand.Connection = sqlConnection;
                sqlCommand.CommandText =
                "DELETE FROM webpages_Membership " +
                " WHERE UserId= " + user.UserId;
                try
                {
                    sqlConnection.Open();
                    sqlCommand.ExecuteNonQuery();

                sqlCommand.CommandText =
                "DELETE FROM webpages_UsersInRoles " +
                " WHERE UserId= " + user.UserId;
                    sqlCommand.ExecuteNonQuery();


                }
                catch (Exception ex)
                {
                }
                finally
                {
                    if (sqlConnection.State == System.Data.ConnectionState.Open)
                        sqlConnection.Close();
                }

                return RedirectToAction("UserManage");
            }

            return HttpNotFound();
        }

        #region Helpers
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

        public enum ManageMessageId
        {
            ChangePasswordSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess,
        }

        internal class ExternalLoginResult : ActionResult
        {
            public ExternalLoginResult(string provider, string returnUrl)
            {
                Provider = provider;
                ReturnUrl = returnUrl;
            }

            public string Provider { get; private set; }
            public string ReturnUrl { get; private set; }

            public override void ExecuteResult(ControllerContext context)
            {
                OAuthWebSecurity.RequestAuthentication(Provider, ReturnUrl);
            }
        }

        private static string ErrorCodeToString(MembershipCreateStatus createStatus)
        {
            switch (createStatus)
            {
                case MembershipCreateStatus.DuplicateUserName:
                    return "Потребителското име е заето. Моля използвайте друго.";

                case MembershipCreateStatus.DuplicateEmail:
                    return "Вече съществува потребителско име с този e-mail. Моля въведете друг.";

                case MembershipCreateStatus.InvalidPassword:
                    return "Предоставената парола е грешна. Моля въведете правилна.";

                case MembershipCreateStatus.InvalidEmail:
                    return "Предоставеният e-mail адрес е грешен. Моля въведете правилен.";

                case MembershipCreateStatus.InvalidAnswer:
                    return "Отговорът за въстановяване на парола е грешен. Моля въведете правилен.";

                case MembershipCreateStatus.InvalidQuestion:
                    return "Въпросът за въстановяване на парола е грешен. Моля въведете правилен.";

                case MembershipCreateStatus.InvalidUserName:
                    return "Въведеното потребителско име е грешно. Моля въведете правилно.";

                case MembershipCreateStatus.ProviderError:
                    return "Възникна грешка. Опитайте отново или се свържете с администратор.";

                case MembershipCreateStatus.UserRejected:
                    return "Възникна грешка. Опитайте отново или се свържете с администратор.";

                default:
                    return "Нещо се счупи. Опитайте отново или се свържете с администратор.";
            }
        }
        #endregion
    }
}
