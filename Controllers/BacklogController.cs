using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PPlanner.Models;
using System.Web.Routing;
using MvcSiteMapProvider;

namespace PPlanner.Controllers
{   
    [Authorize]
    public class BacklogController : Controller
    {
        private ProjectDbContext db = new ProjectDbContext();
        
        public class MyAuthorizeAttribute : AuthorizeAttribute
        {
            protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
            {
                filterContext.Result = new RedirectToRouteResult(
                                   new RouteValueDictionary
                                   {
                                       { "action", "Details" },
                                       { "controller", "Project"}
                                   });
            }
        }
        public void GetProjectNameForBreadcrumbs(Project project)
        {
            var node = SiteMaps.Current.CurrentNode;

            if (node != null && node.ParentNode.Title != null)
            {
                node.ParentNode.Title = project.Name; 
                node.ParentNode.Description = project.ProjectId.ToString();

            }
        }

        //
        // GET: /Backlog/
        public ActionResult Index(int ProjectId = 0)
        {

            if (ProjectId > 0)
                HttpContext.Session["CurrentProject"] = ProjectId.ToString();
            else if (HttpContext.Session["CurrentProject"] == null)
                Redirect("/Project/Index");
            else
                ProjectId = Int32.Parse(HttpContext.Session["CurrentProject"].ToString());

            ViewBag.CurrentProject = ProjectId.ToString();
            ViewBag.ProjectName = db.Projects.Find(ProjectId).Name;

            var userstories = db.UserStories
                                .Where(u => u.Project_ProjectId == ProjectId)
                                .OrderBy(u => u.BackLogPriority);


            int currentSprintId = db.Sprints.Where(cs => cs.Project_ProjectId == ProjectId && cs.StartDate <= DateTime.Today && cs.EndDate >= DateTime.Today).Select(cs => cs.SprintId).FirstOrDefault();

            List<Sprint> projectSprints = db.Sprints.Where(ur => ur.Project_ProjectId == ProjectId).ToList();

            foreach (var item in projectSprints)
            {
                if (item.SprintId == currentSprintId)
                {
                    item.IsCurrent = true;
                    db.Entry(item).State = EntityState.Modified;
                    db.SaveChanges();
                }
                else
                {
                    item.IsCurrent = false;
                    db.Entry(item).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }

            

            List<Sprint> sprints = db.Sprints
                                    .Where(s => s.Project_ProjectId == ProjectId)
                                    .OrderBy(s => s.SprintId)
                                    .ToList();

            

            ViewBag.Sprint_SprintId = new SelectList(sprints, "SprintId", "Name");

            //ViewBag.SprintSprintId = new SelectList(db.Sprints, "SprintId", "Name", userstory.SprintSprintId);

            Project project = db.Projects.Where(p => p.ProjectId == ProjectId).FirstOrDefault();
            if (project.CurrentSprint_SprintId != currentSprintId)
            {
                project.CurrentSprint_SprintId = currentSprintId;
            }            
            
            GetProjectNameForBreadcrumbs(project);

            return View(userstories.ToList());
        }

        //
        public ActionResult Complete(int id)
        {
            UserStory userstory = db.UserStories.Find(id);
            userstory.CompletedDate = DateTime.Today;
            if (userstory.User_Effort == userstory.Effort)
            {
                userstory.User_Effort = userstory.Effort;
            }
            userstory.ItemStatus_ItemStatusId = 3;

            db.Entry(userstory).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("Details", new { id = userstory.Id });
        }

        // GET: /Backlog/Details/5
        public ActionResult Details(int id = 0)
        {
            UserStory userstory = db.UserStories.Find(id);
            if (userstory == null)
            {
                return HttpNotFound();
            }

            Project project = db.Projects.Where(p => p.ProjectId == userstory.Project_ProjectId).FirstOrDefault();

            GetProjectNameForBreadcrumbs(project);

            return View(userstory);
        }

        ////
        // GET: /Backlog/Create
        public ActionResult Create()
        {

            ViewBag.ProjectProjectId = new SelectList(db.Projects, "ProjectId", "Name");
            ViewBag.ItemStatusItemStatusId = new SelectList(db.ItemStatuses, "ItemStatusId", "Status");
            ViewBag.SprintSprintId = new SelectList(db.Sprints, "SprintId", "Name");

            return View();
        }

        //
        // POST: /Backlog/Create
        [HttpPost]
        public ActionResult Create(UserStory userstory)
        {
            if (ModelState.IsValid)
            {

                userstory.BackLogPriority = db.UserStories.Where(i => i.Project_ProjectId == userstory.Project_ProjectId).Count() + 1;

                userstory.ItemStatus_ItemStatusId = 1;

                //Project p1 = db.Projects.Find(userstory.Project_ProjectId);

                userstory.Sprint = null;
                userstory.Sprint_SprintId = null;

                //userstory.Sprint_SprintId = db.Projects.Single(p => p.ProjectId == userstory.Project_ProjectId)
                //                            .Sprints.OrderBy(s => s.SprintId).First().SprintId;

                db.UserStories.Add(userstory);
                db.SaveChanges();
                return RedirectToAction("Index", new { ProjectId = userstory.Project_ProjectId });
            }

            //ViewBag.ProjectProjectId = new SelectList(db.Projects, "ProjectId", "Name", userstory.ProjectProjectId);
            //ViewBag.ItemStatusItemStatusId = new SelectList(db.ItemStatuses, "ItemStatusId", "Status", userstory.ItemStatusItemStatusId);
            //ViewBag.SprintSprintId = new SelectList(db.Sprints, "SprintId", "Name", userstory.SprintSprintId);

            return RedirectToAction("Index", new { ProjectId = userstory.Project_ProjectId });
            //return View(userstory);
        }

        //
        // GET: /Backlog/Edit/5

        public ActionResult Edit(int id = 0)
        {
            UserStory userstory = db.UserStories.Find(id);
            if (userstory == null)
            {
                return HttpNotFound();
            }

            //ViewBag.ProjectProjectId = new SelectList(db.Projects, "ProjectId", "Name", userstory.Project_ProjectId);
            ViewBag.ItemStatus_ItemStatusId = new SelectList(db.ItemStatuses, "ItemStatusId", "Status", userstory.ItemStatus_ItemStatusId);
            ViewBag.Sprint_SprintId = new SelectList(db.Sprints.Where(s => s.Project_ProjectId == userstory.Project_ProjectId).ToList(),
                                        "SprintId", "Name", userstory.Sprint_SprintId);
            List<int> devsInProject = db.ProjectDevelopers.Where(dip => dip.ProjectId == userstory.Project_ProjectId).Select(dip => dip.DevId).ToList();
            ViewBag.UserProfile_UserId = new SelectList(db.UserProfiles.Where(up => up.isApproved && devsInProject.Contains(up.UserId)).ToList(),
                                                        "UserId", "UserName", userstory.UserProfile_UserId);

            return View(userstory);
        }

        //
        // POST: /Backlog/Edit/5

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
                return RedirectToAction("Index", new { ProjectId = userstory.Project_ProjectId });
            }
            
            //ViewBag.ProjectProjectId = new SelectList(db.Projects, "ProjectId", "Name", userstory.Project_ProjectId);

            ViewBag.ItemStatus_ItemStatusId = new SelectList(db.ItemStatuses, "ItemStatusId", "Status", userstory.ItemStatus_ItemStatusId);
            ViewBag.Sprint_SprintId = new SelectList(db.Sprints.Where(s => s.Project_ProjectId == userstory.Project_ProjectId).ToList(),
                                        "SprintId", "Name", userstory.Sprint_SprintId);
            ViewBag.UserProfile_UserId = new SelectList(db.UserProfiles.Where(up => up.isApproved).ToList(),
                                            "UserId", "UserName", userstory.UserProfile_UserId);

            return View(userstory);
        }


        //
        // GET: /Backlog/UserEdit/5
        [MyAuthorize(Roles="Developer")]
        public ActionResult UserEdit(int id = 0)
        {
            UserStory userstory = db.UserStories.Find(id);
            if (userstory == null)
            {
                return HttpNotFound();
            }

            //ViewBag.ProjectProjectId = new SelectList(db.Projects, "ProjectId", "Name", userstory.Project_ProjectId);
            ViewBag.ItemStatus_ItemStatusId = new SelectList(db.ItemStatuses, "ItemStatusId", "Status", userstory.ItemStatus_ItemStatusId);
            ViewBag.Sprint_SprintId = new SelectList(db.Sprints.Where(s => s.Project_ProjectId == userstory.Project_ProjectId).ToList(),
                                        "SprintId", "Name", userstory.Sprint_SprintId);
            ViewBag.UserProfile_UserId = new SelectList(db.UserProfiles.Where(up => up.isApproved).ToList(),
                                                        "UserId", "UserName", userstory.UserProfile_UserId);

            return View(userstory);
        }


        // POST: /Backlog/Edit/5

        [HttpPost]
        public ActionResult UserEdit(UserStory userstory)
        {
            if (ModelState.IsValid)
            {
                if (userstory.ItemStatus_ItemStatusId == 3)
                {
                    userstory.CompletedDate = DateTime.Today;
                }

                db.Entry(userstory).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index", new { ProjectId = userstory.Project_ProjectId });
            }

            //ViewBag.ProjectProjectId = new SelectList(db.Projects, "ProjectId", "Name", userstory.Project_ProjectId);

            ViewBag.ItemStatus_ItemStatusId = new SelectList(db.ItemStatuses, "ItemStatusId", "Status", userstory.ItemStatus_ItemStatusId);
            ViewBag.Sprint_SprintId = new SelectList(db.Sprints.Where(s => s.Project_ProjectId == userstory.Project_ProjectId).ToList(),
                                        "SprintId", "Name", userstory.Sprint_SprintId);
            ViewBag.UserProfile_UserId = new SelectList(db.UserProfiles.Where(up => up.isApproved).ToList(),
                                            "UserId", "UserName", userstory.UserProfile_UserId);

            return View(userstory);
        }

        //
        // GET: /Backlog/Delete/5

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
        // POST: /Backlog/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            UserStory userstory = db.UserStories.Find(id);
            userstory.ItemStatus_ItemStatusId = 4;
            db.Entry(userstory).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("Index", new { ProjectId = userstory.Project_ProjectId });
        }

        [HttpPost]
        public string setSprint(int ProjectId, int SprintId, int UserStoryId)
        {
            UserStory userstory = db.UserStories
                                    .Where(us => us.Id == UserStoryId)
                                    .FirstOrDefault() ?? null;

            Sprint sprint = db.Sprints
                               .Where(s => s.SprintId == SprintId)
                               .FirstOrDefault() ?? null;

            if (userstory == null || sprint == null)
                return null;

            userstory.Sprint_SprintId = sprint.SprintId;
            userstory.Sprint = sprint;

            db.Entry(userstory).State = EntityState.Modified;

            db.SaveChanges();

            return sprint.Name;
        }

        [HttpPost]
        public void PrioritySort(int position, int id = 0)
        {
            if (id > 0)
            {
                UserStory userstory = db.UserStories.Find(id);
                //userstory.ProjectId = db.Projects.Single(i => i.ProjectId == (int)HttpContext.Session["CurrentProject"]);

                if (userstory != null)
                {
                    int p = position;
                    List<UserStory> usg;
                    if (userstory.BackLogPriority < position)
                    {
                        usg = db.UserStories.Where(i => i.Project_ProjectId == userstory.Project_ProjectId
                                                        && i.BackLogPriority <= position
                                                        && i.BackLogPriority > userstory.BackLogPriority)
                                                        .OrderByDescending(i => i.BackLogPriority)
                                                        .ToList();

                        foreach (UserStory item in usg)
                        {
                            //if (p == 0) break;
                            item.BackLogPriority = --p;
                            db.Entry(item).State = EntityState.Modified;
                        }

                    }
                    else if (userstory.BackLogPriority > position)
                    {
                        usg = db.UserStories.Where(i => i.Project_ProjectId == userstory.Project_ProjectId
                                                            && i.BackLogPriority >= position
                                                            && i.BackLogPriority < userstory.BackLogPriority)
                                                            .OrderBy(i => i.BackLogPriority).ToList();

                        foreach (UserStory item in usg)
                        {
                            item.BackLogPriority = ++p;
                            db.Entry(item).State = EntityState.Modified;
                        }
                    }


                    userstory.BackLogPriority = position;
                    db.Entry(userstory).State = EntityState.Modified;

                    //save back to database.
                    db.SaveChanges();
                }
            }
        }


        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}