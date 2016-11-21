using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PPlanner.Models;
using WebMatrix.WebData;
using System.Web.Routing;

namespace PPlanner.Controllers
{
        [Authorize]
    public class SprintController : Controller
    {
        private ProjectDbContext db = new ProjectDbContext();

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

        //
        // GET: /Sprint/

        public ActionResult Index(int ProjectId, int SprintId)
        {
            if (ProjectId < 0)
            {
                return HttpNotFound();
            }

            var userstories = db.UserStories.Where(u => u.Project_ProjectId == ProjectId && u.Sprint_SprintId == SprintId)
                                .OrderBy(u => u.BackLogPriority)
                                .Include(u => u.ItemStatus);

            ViewBag.CurrentProject = ProjectId;

            ViewBag.ProjectName = db.Projects.Find(ProjectId).Name;
            ViewBag.SprintName = db.Sprints.Find(SprintId).Name;

            return View(userstories.ToList());
        }

        //
        // GET: /Sprint/Details/5

        public ActionResult Details(int id = 0)
        {
            UserStory userstory = db.UserStories.Find(id);
            if (userstory == null)
            {
                return HttpNotFound();
            }
            return View(userstory);
        }

        //
        // GET: /Sprint/Create

        public ActionResult Create()
        {
            ViewBag.ProjectProjectId = new SelectList(db.Projects, "ProjectId", "Name");
            ViewBag.ItemStatusItemStatusId = new SelectList(db.ItemStatuses, "ItemStatusId", "Status");
            ViewBag.SprintSprintId = new SelectList(db.Sprints, "SprintId", "Name");
            return View();
        }

        //
        // POST: /Sprint/Create

        [HttpPost]
        public ActionResult Create(UserStory userstory)
        {
            if (ModelState.IsValid)
            {
                db.UserStories.Add(userstory);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.ProjectProjectId = new SelectList(db.Projects, "ProjectId", "Name", userstory.Project.ProjectId);
            ViewBag.ItemStatusItemStatusId = new SelectList(db.ItemStatuses, "ItemStatusId", "Status", userstory.ItemStatus.ItemStatusId);
            ViewBag.SprintSprintId = new SelectList(db.Sprints, "SprintId", "Name", userstory.Sprint.SprintId);
            return View(userstory);
        }

        //
        // GET: /Sprint/Edit/5

        [MyAuthorize(Roles = "Scrum master")]
        public ActionResult Edit(int id = 0)
        {
            UserStory userstory = db.UserStories.Find(id);
            if (userstory == null)
            {
                return HttpNotFound();
            }
            //ViewBag.ProjectProjectId = new SelectList(db.Projects, "ProjectId", "Name", userstory.Project.ProjectId);

            ViewBag.ItemStatus_ItemStatusId = new SelectList(db.ItemStatuses, "ItemStatusId", "Status", userstory.ItemStatus_ItemStatusId);
            
            //ViewBag.Sprint_SprintId = new SelectList(db.Sprints.Where(s => s.Project_ProjectId == userstory.Project_ProjectId).ToList(),
            //                            "SprintId", "Name", userstory.Sprint_SprintId);
            ViewBag.UserProfile_UserId = new SelectList(db.UserProfiles.Where(up => up.isApproved).ToList(),
                                            "UserId", "UserName", userstory.UserProfile_UserId);

            return View(userstory);
        }

        //
        // POST: /Sprint/Edit/5

        [HttpPost]
        public ActionResult Edit(UserStory userstory)
        {
            if (ModelState.IsValid)
            {
                if (userstory.ItemStatus_ItemStatusId == 3)
                {
                    userstory.CompletedDate = DateTime.Today;
                }

                db.Entry(userstory).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index", new { ProjectId = userstory.Project_ProjectId, SprintId = userstory.Sprint_SprintId });
            }
            //ViewBag.ProjectProjectId = new SelectList(db.Projects, "ProjectId", "Name", userstory.Project.ProjectId);
            ViewBag.ItemStatus_ItemStatusId = new SelectList(db.ItemStatuses, "ItemStatusId", "Status", userstory.ItemStatus_ItemStatusId);
            //ViewBag.Sprint_SprintId = new SelectList(db.Sprints.Where(s => s.Project_ProjectId == userstory.Project_ProjectId).ToList(),
            //                            "SprintId", "Name", userstory.Sprint_SprintId);
            ViewBag.UserProfile_UserId = new SelectList(db.UserProfiles.Where(up => up.isApproved).ToList(),
                                            "UserId", "UserName", userstory.UserProfile_UserId);

            return View(userstory);
        }

        //
        // GET: /Sprint/Delete/5
        [MyAuthorize(Roles = "Scrum master")]
        public ActionResult Delete(int id = 0)
        {
            UserStory userstory = db.UserStories.Find(id);
            if (userstory == null)
            {
                return HttpNotFound();
            }
            return View(userstory);
        }

        //
        // POST: /Sprint/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            UserStory userstory = db.UserStories.Find(id);
            db.UserStories.Remove(userstory);
            db.SaveChanges();
            return RedirectToAction("Index");
        }


        public ActionResult SprintBoard(int ProjectId = 0)
        {
            Sprint currentSprint = db.Sprints.Where(sp => sp.Project_ProjectId == ProjectId
                && sp.StartDate <= DateTime.Today 
                && sp.EndDate >= DateTime.Today).FirstOrDefault() ?? null;

            List<UserStory> userstories = null;
            var uid = WebSecurity.GetUserId(User.Identity.Name);

            if (currentSprint != null)
            {
                if (User.IsInRole("Scrum master"))
                {
                    userstories = db.UserStories
                        .Where(us => us.Project_ProjectId == ProjectId && us.Sprint_SprintId == currentSprint.SprintId).ToList();
                }
                else
                {
                    userstories = db.UserStories
                                            .Where(us => us.Project_ProjectId == ProjectId 
                                            && us.Sprint_SprintId == currentSprint.SprintId 
                                            && us.UserProfile_UserId == uid).ToList();
                }

                ViewBag.currentSprint = currentSprint.Name;
                ViewBag.ToDoStatus = db.ItemStatuses.Where(i => i.ItemStatusId == 1).Select(i => i.ItemStatusId).FirstOrDefault();
                ViewBag.InProgress = db.ItemStatuses.Where(i => i.ItemStatusId == 2).Select(i => i.ItemStatusId).FirstOrDefault();
                ViewBag.Done = db.ItemStatuses.Where(i => i.ItemStatusId == 3).Select(i => i.ItemStatusId).FirstOrDefault();

                ViewBag.ProjectId = ProjectId;
                ViewBag.ProjectName = db.Projects.Where(pr=>pr.ProjectId.Equals(ProjectId)).Select(pr=>pr.Name).FirstOrDefault();
            }
            else
            {
                userstories = db.UserStories
                .Where(us => us.Project_ProjectId == 0).ToList();

                ViewBag.currentSprint = "";
                ViewBag.ToDoStatus = 1;
                ViewBag.InProgress = 1;
                ViewBag.Done = 1;
                ViewBag.ProjectId = ProjectId;
                ViewBag.ProjectName = db.Projects.Where(pr => pr.ProjectId.Equals(ProjectId)).Select(pr => pr.Name).FirstOrDefault();
            }


            return View(userstories);
        }

        public ActionResult UserList(int ProjectId)
        {
            List<int> usersInProject = db.ProjectDevelopers.Where(uip => uip.ProjectId == ProjectId).Select(uip => uip.DevId).ToList();
            List<UserProfile> userprofiles = db.UserProfiles
                             .Where(up => up.isApproved && usersInProject.Contains(up.UserId))
                             .OrderBy(up => up.UserName).ToList();

            return PartialView("_UserList", userprofiles);
        }

        [HttpPost]
        public string setUser(int UserId, int UserStoryId)
        {
            UserStory userstory = db.UserStories
                                    .Where(us => us.Id == UserStoryId)
                                    .FirstOrDefault() ?? null;

            UserProfile user = db.UserProfiles
                               .Where(u => u.UserId == UserId)
                               .FirstOrDefault() ?? null;

            if (userstory == null || user == null)
                return "";

            userstory.UserProfile_UserId = user.UserId;
            userstory.UserProfile = user;

            db.Entry(userstory).State = EntityState.Modified;
            db.SaveChanges();

            return user.UserName;
        }

        [HttpPost]
        public string SetStatus(int UserStoryId, int StatusId)
        {
            UserStory userstory = db.UserStories.Find(UserStoryId);
                                    //.Where(us => us.Id == UserStoryId)
                                    //.FirstOrDefault() ?? null;
            ItemStatus itemstatus = db.ItemStatuses.Find(StatusId);

            if (userstory == null || itemstatus == null)
            {
                return "";
            }

            if (itemstatus.Status.ToLower().Equals("Done"))
            {
                userstory.CompletedDate = DateTime.Today;
            }
            else
            {
                userstory.CompletedDate = null;
            }

            userstory.ItemStatus_ItemStatusId = itemstatus.ItemStatusId;
            userstory.ItemStatus = itemstatus;

            db.Entry(userstory).State = EntityState.Modified;
            db.SaveChanges();

            return "Changed";
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}