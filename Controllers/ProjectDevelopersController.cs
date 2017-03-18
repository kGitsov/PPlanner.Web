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
    public class ProjectDevelopersController : Controller
    {
        ProjectDbContext db = new ProjectDbContext();
        //
        // GET: /Developers/

        public ActionResult Index()
        {
            
            return View();
        }

        public ActionResult AddNewDeveloper(string ProjectId)
        {
            ViewBag.ProjectId = ProjectId;
            ViewBag.UsersList = new SelectList(db.UserProfiles.Where(us => us.RoleName != "Scrum master").Select(us => us.UserName), "UserName");
            ViewBag.RoleName = new SelectList(Roles.GetAllRoles(), "RoleName");
            int id = Convert.ToInt32(ProjectId);
            ViewBag.ProjectName = db.Projects.Where(pn => pn.ProjectId == id).Select(pn => pn.Name).FirstOrDefault().ToString();
            
            return View();
        }

        [HttpPost]
        public ActionResult Create(string UsersList, string RoleName, string ProjectId)
        {
            ProjectDevelopers newDev = new ProjectDevelopers();
            newDev.PositionName = RoleName;
            newDev.ProjectId = Convert.ToInt32(ProjectId);
            newDev.DevId = db.UserProfiles.Where(di => di.UserName == UsersList).Select(di => di.UserId).FirstOrDefault();
            int isTrue = db.ProjectDevelopers.Where(it => it.DevId == newDev.DevId && it.ProjectId == newDev.ProjectId).Count();

            if (isTrue < 1)
            {
                db.ProjectDevelopers.Add(newDev);
                db.SaveChanges();
                return RedirectToAction("Details", "Project", new { id = ProjectId });
            }
            else
            {
                var message = db.Resources.Where(mg => mg.ResName == "ProjectDetailsDevAlreadyIn").FirstOrDefault();
                TempData["msg"] = "<script>alert('" + message.ResValue + "');</script>";
                return RedirectToAction("Details", "Project", new { id = ProjectId });
                
            }
        }

        public ActionResult Delete(int id = 0, int ProjectId = 0)
        {
            ProjectDevelopers delDev = new ProjectDevelopers();
            var inUse = db.UserStories.Where(ui => ui.UserProfile_UserId == id).Count();
            delDev = db.ProjectDevelopers.Where(dd => dd.DevId == id && dd.ProjectId == ProjectId).FirstOrDefault();
            if (inUse < 1)
            {
                db.ProjectDevelopers.Remove(delDev);
                db.SaveChanges();
                return View();
            }
            else
            {
                var message = db.Resources.Where(mg => mg.ResName == "ProjectDetailsDevInUse").FirstOrDefault();
                TempData["msg"] = "<script>alert('" + message.ResValue + "');</script>";
                return RedirectToAction("Details", "Project", new { id = ProjectId });
            }
        }
    }
}
