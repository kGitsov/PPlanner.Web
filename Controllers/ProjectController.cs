using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PPlanner.Models;
using System.Globalization;
using System.Web.Security;
using System.Web.Routing;
using MvcSiteMapProvider;
using System.Collections;
using System.Web.Helpers;
using System.IO;
using PagedList;

namespace PPlanner.Controllers
{
    [Authorize]
    public class ProjectController : Controller
    {
        private ProjectDbContext db = new ProjectDbContext();

        //Check if user has rights to this project
        public bool CheckProjectAuthentication(int cp)
        {
            //Get current's user UserId
            int userId = db.UserProfiles.Where(uid => uid.UserName == User.Identity.Name).Select(uid => uid.UserId).FirstOrDefault();
            //Get projectIds for current user
            List<int> projectIds = new List<int>(db.UserStories.Where(up => up.UserProfile_UserId == userId).Select(up => up.Project_ProjectId).Distinct());
            List<int> smProjectIds = new List<int>(db.Projects.Where(smp => smp.SM_UserId == userId).Select(smp => smp.ProjectId).Distinct());
            List<int> poProjectIds = new List<int>(db.ProjectDevelopers.Where(pop => pop.DevId == userId).Select(pop => pop.ProjectId).Distinct());
            if (projectIds.Contains(cp)||smProjectIds.Contains(cp) || poProjectIds.Contains(cp))
            {
                return true;
            }
            else { return false; }
        }

        public void GetProjectNameForBreadcrumbs(Project project)
        {
            var node = SiteMaps.Current.CurrentNode;

            if (node != null && node.ParentNode != null)
            {
                node.ParentNode.Title = project.Name;

            }
        }

        //override unauthorizedrequest in projects
        public class MyAuthorizeAttribute : AuthorizeAttribute
        {
            protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
            {
                filterContext.Result = new RedirectToRouteResult(
                                   new RouteValueDictionary
                                   {
                                       { "action", "Index" },
                                       { "controller", "Project" }
                                   });
            }
        }

        //
        // GET: /Project/
        public ActionResult Index(int? page)
        {
            
            //Guid userGuid = (Guid)Membership.GetUser().ProviderUserKey;
            if (!User.IsInRole("Admin"))
            {
                //Get current's user UserId
                int userId = db.UserProfiles.Where(uid => uid.UserName == User.Identity.Name).Select(uid => uid.UserId).FirstOrDefault();

                List<int> projectIds = new List<int>();

                if (!User.IsInRole("Scrum master"))
                {
                    //Get projectIds for current developer
                    projectIds = new List<int>(db.UserStories.Where(up => up.UserProfile_UserId == userId).Select(up => up.Project_ProjectId).Distinct());
                    projectIds.AddRange(db.ProjectDevelopers.Where(pd => pd.DevId == userId).Select(pd => pd.ProjectId).ToList());
                    projectIds = projectIds.Distinct().ToList();

                }
                else
                {
                    //Get projectIds for current scrummaster
                    projectIds = new List<int>(db.Projects.Where(up => up.SM_UserId == userId).Select(up => up.ProjectId).Distinct());
                }

                List<Project> userProjects = new List<Project>();

                foreach (var item in projectIds)
                {
                    Project pr = db.Projects.Where(p => p.ProjectId == item).FirstOrDefault();
                    userProjects.Add(pr);
                }

                ViewBag.NumOfTasksToday = db.UserStories
                .Where(us => us.UserProfile_UserId == WebMatrix.WebData.WebSecurity.CurrentUserId
                && us.CompletedDate == null && us.Sprint.EndDate >= DateTime.Today).Count();

                return View(userProjects);
            }
            ViewBag.NumOfTasksToday = db.UserStories
                .Where(us => us.UserProfile_UserId == WebMatrix.WebData.WebSecurity.CurrentUserId
                && us.CompletedDate == null && us.Sprint.EndDate >= DateTime.Today).Count();
            int pageSize = 5;
            int pageNumber = (page ?? 1);
            var projectss = db.Projects.OrderBy(pr => pr.ProjectId);
            return View(projectss.ToPagedList(pageNumber, pageSize));
        }


        [MyAuthorize(Roles = "Product owner")]
        public ActionResult Undone(string ProjectId)
        {
            
            Project getProject = db.Projects.Find(Convert.ToInt32(ProjectId));
            ViewBag.ProjectName = getProject.Name;
            ViewBag.ProjectId = getProject.ProjectId;
            ViewBag.StartDate = getProject.StartDate;
            ViewBag.EndDate = getProject.EndDate;
            ViewBag.CanceledTasks = db.UserStories.Where(ct => ct.Project_ProjectId == getProject.ProjectId && ct.ItemStatus_ItemStatusId == 4 && ct.CompletedDate == null).Count();
            ViewBag.UndoneTasks = db.UserStories.Where(ut => ut.Project_ProjectId == getProject.ProjectId && ut.ItemStatus_ItemStatusId != 3 && ut.ItemStatus_ItemStatusId != 4 && ut.CompletedDate == null).Count();
            ViewBag.WorkingButNotDone = db.UserStories.Where(wd => wd.Project_ProjectId == getProject.ProjectId && wd.ItemStatus_ItemStatusId == 2 && wd.CompletedDate == null).Count();

            List<UserStory> getNotDoneUserStories = db.UserStories.Where(nd => nd.Project_ProjectId == getProject.ProjectId && nd.ItemStatus_ItemStatusId != 3 && nd.CompletedDate == null && nd.Project.EndDate < DateTime.Today).ToList();
            if (getNotDoneUserStories.Count > 0)
            {
                return View(getNotDoneUserStories);
            }
            else
            {
                return View();
            }
            
        }

        //
        // GET: /Project/Details/5
        public ActionResult Details(int id = 0)
        {
             
            if (id > 0)
            {
                if (CheckProjectAuthentication(id) || User.IsInRole("Admin"))
                {
                    Project project = db.Projects.Find(id);

                    if (project == null)
                    {
                        return HttpNotFound();
                    }

                    Sprint sprint = db.Sprints
                                    .Where(s => s.Project_ProjectId == id && s.StartDate <= DateTime.Today && s.EndDate >= DateTime.Today)
                                    .FirstOrDefault() ?? null;

                    if (sprint != null)
                    {
                        ViewBag.totalDays = numberOfDays(project.StartDate, project.EndDate);
                        ViewBag.remainingDays = numberOfDays(project.StartDate, project.EndDate) - numberOfDays(project.StartDate, DateTime.Today);
                        ViewBag.NoUserstories = db.UserStories.Where(nu => nu.Project_ProjectId == id).Count();
                        ViewBag.currentSprint = sprint.Name;
                        ViewBag.sprintStart = sprint.StartDate.ToString("dd/MM/yyyy");
                        ViewBag.sprintEnd = sprint.EndDate.ToString("dd/MM/yyyy");
                        ViewBag.currentSprintCountTasks = db.UserStories.Where(cs => cs.Project_ProjectId == id &&
                                                                                        cs.Sprint_SprintId == sprint.SprintId && cs.ItemStatus_ItemStatusId < 3).Count();
                        //ViewBag.sprintStart = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(sprint.StartDate.Month) + " " + sprint.StartDate.Day;
                        //ViewBag.sprintEnd = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(sprint.EndDate.Month) + " " + sprint.EndDate.Day;
                    }

                    GetProjectNameForBreadcrumbs(project);

                   
                    var chartKey = project.ProjectId.ToString();

                    if (chartKey != null)
                    {
                        var cachedChart = Chart.GetFromCache(key: chartKey);

                        if (cachedChart == null)
                        {

                            int countx = db.UserStories.Where(us => us.Project_ProjectId == project.ProjectId).Count();
                            int county, maxy, maxyu;

                            if (db.UserStories.Where(us => us.Project_ProjectId == project.ProjectId).Count() <= 0)
                            {
                                maxy = 0;
                                maxyu = 0;
                            }
                            else
                            {
                                maxy = db.UserStories.Where(us => us.Project_ProjectId == project.ProjectId).Select(us => us.Effort).Max();
                                maxyu = db.UserStories.Where(us => us.Project_ProjectId == project.ProjectId).Select(us => us.User_Effort).Max();
                            }

                            if (maxy >= maxyu) {
                                county = maxy;
                            }
                            else {
                                county = maxyu;
                            }

                            cachedChart = new Chart(468, 263);

                            cachedChart.SetXAxis("Tasks", 0, countx+1);
                            cachedChart.SetYAxis("Effort", 0, county);
                            
                            var xvalues = db.UserStories.Where(usx => usx.Project_ProjectId == project.ProjectId).Select(usx => usx.BackLogPriority);

                            Dictionary<int, int> yvaluesSM = new Dictionary<int, int>();
                            List<int> ysmbp = new List<int>(db.UserStories.Where(usy => usy.Project_ProjectId == project.ProjectId).Select(usy => usy.BackLogPriority).ToList());
                            List<int> ysme = new List<int>(db.UserStories.Where(usy => usy.Project_ProjectId == project.ProjectId).Select(usy => usy.Effort).ToList());

                            //var yvaluesUE = ;
                            Dictionary<int, int> yvaluesUE = new Dictionary<int, int>();
                            List<int> yuee = new List<int>(db.UserStories.Where(usyu => usyu.Project_ProjectId == project.ProjectId).Select(usyu => usyu.User_Effort).ToList());

                            for (int i = 0; i < ysme.Count; i++)
                            {
                                yvaluesSM.Add(ysmbp[i], ysme[i]);
                                yvaluesUE.Add(ysmbp[i], yuee[i]);
                            }

                            yvaluesSM.OrderBy(key => key.Key);
                            yvaluesUE.OrderBy(key => key.Key);

                            var ys = yvaluesSM.Values;
                            var ye = yvaluesUE.Values;
                            //var two = new[] { yvaluesSM.ToArray(), yvaluesUE.ToArray() };

                            cachedChart.AddSeries(
                                name: "SM",
                                chartType: "Column",
                                axisLabel: "",
                                xValue: xvalues.ToArray(),
                                yValues: ys.ToArray()
                                );
                            cachedChart.AddSeries(
                                name: "UE",
                                chartType: "Column",
                                axisLabel: "",
                                xValue: xvalues.ToArray(),
                                yValues: ye.ToArray()
                                );
                            cachedChart.SaveToCache(key: chartKey,
                                minutesToCache: 1,                                
                                slidingExpiration: true);
                        }
                                                
                        byte[] uImage = new byte[cachedChart.ToWebImage("jpeg").GetBytes().Length];
                        uImage = cachedChart.ToWebImage("jpeg").GetBytes();
                        var img = String.Format("data:image/jpg;base64,{0}", Convert.ToBase64String(uImage));
                        ViewBag.ChartData = img;
                        
                    }
                    



                    
                    return View(project);
                }
                else
                {
                    Project project = new Project();
                    return View(project);
                }
            }
            else
            {
                Project project = new Project();
                return View(project);
            }  

        }

        public ActionResult ListDevelopers (int ProjectId)
        {
            List<int> userIds = new List<int>();
            userIds = db.ProjectDevelopers.Where(pd => pd.ProjectId == ProjectId).Select(pd => pd.DevId).ToList();
            if (userIds != null)
            {
                var users = new List<Tuple<UserProfile, int, int, int>>();

                foreach (var item in userIds)
                {
                    int num1 = db.UserStories.Where(nm => nm.UserProfile_UserId == item && nm.Project_ProjectId == ProjectId).Count();
                    int num2 = db.UserStories.Where(nm => nm.ItemStatus_ItemStatusId < 3 && nm.Project_ProjectId == ProjectId && nm.UserProfile_UserId == item).Count();
                    UserProfile temp = db.UserProfiles.Find(item);

                    users.Add(Tuple.Create(temp, num1, num2, item));
                }
                ViewBag.Id = ProjectId;
                if (users != null)
                {
                    return PartialView("_ListDevelopers", users);
               }
                else
                {
                    return PartialView("_ListDevelopers");
                }
                
            }
            else
            {
                return View();
            }

        }

        public ActionResult Tasks()
        {
             
            return View();
        }

        //
        // GET: /Project/Create
        [MyAuthorize(Roles = "Admin")]
        public ActionResult Create()
        {
             
            Project newPrj = new Project();

            newPrj.NumOfIterationDay = 30;
            newPrj.StartDate = DateTime.Today;
            newPrj.EndDate = businessDaysFromStartDate(DateTime.Today, 30);
            newPrj.WorkingDaysOnly = true;

            return View(newPrj);
        }

        //
        // POST: /Project/Create        
        [HttpPost]
        [MyAuthorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Project project)
        {
             
            if (ModelState.IsValid)
            {
                //TODO end date must be greater than start date

                //TODO get sprint list

                List<Sprint> sprints = getSprints(project.StartDate,
                    project.EndDate, project.NumOfIterationDay, project.WorkingDaysOnly);

                for (int i = 0; i < sprints.Count; i++)
                {
                    sprints[i].Name = "S" + (i + 1) +" " + sprints[i].StartDate.ToString("dd.MM.yy")+ " - "+ sprints[i].EndDate.ToString("dd.MM.yy");
                    sprints[i].Project_ProjectId = project.ProjectId;
                    sprints[i].Project = project;

                    project.Sprints.Add(sprints[i]);
                    db.Sprints.Add(sprints[i]);
                }

                db.Projects.Add(project);

                //db.Sprints.Add(s1);

                db.SaveChanges();
                return RedirectToAction("Index");
            }

            //return RedirectToAction("Index");
            return View(project);
        }

        //
        // GET: /Project/Edit/5
        [MyAuthorize(Roles="Admin")]
        public ActionResult Edit(int id = 0)
        {
             
            Project project = db.Projects.Find(id);

            //
            // new System.Web.Mvc.SelectList(System.Web.Security.Roles.GetAllRoles(), "RoleName");
            //ViewData["RoleName"] = RolesList;
            IEnumerable<SelectListItem> SCMs = new SelectList(db.UserProfiles.Where(up => up.RoleName == "Scrum master").ToList(), "UserId", "UserName");
            ViewData["ScrumMasters"] = SCMs;
            if (project.SM_UserId > 0)
            {
                ViewData["ScrumMaster"] = db.UserProfiles.Where(up => up.UserId == project.SM_UserId).FirstOrDefault().UserName;
            }
            else {
                ViewData["ScrumMaster"] = "Моля изберете ...";
            }
            
            if (project == null)
            {
                return HttpNotFound();
            }

            GetProjectNameForBreadcrumbs(project);

            return View(project);
        }

        //
        // POST: /Project/Edit/5

        [HttpPost]
        public ActionResult Edit(Project project)
        {
             
            //var errors = ModelState.Values.SelectMany(v => v.Errors);
            if (ModelState.IsValid)
            {
                //project.SM_UserId = 1;
                db.Entry(project).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return RedirectToAction("Index");

            //return View(project);
        }

        //
        // GET: /Project/Delete/5

        [MyAuthorize(Roles="Admin")]
        public ActionResult Delete(int id = 0)
        {
             
            Project project = db.Projects.Find(id);
            if (project == null)
            {
                return HttpNotFound();
            }
            GetProjectNameForBreadcrumbs(project);
            return View(project);
        }

        //
        // POST: /Project/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
             
            Project project = db.Projects.Find(id);

            project.Sprints.ToList().ForEach(s => db.Sprints.Remove(s));
            project.UserStories.ToList().ForEach(us => db.UserStories.Remove(us));

            db.Projects.Remove(project);

            db.SaveChanges();
            return RedirectToAction("Index");
        }

        [MyAuthorize(Roles="Admin, Scrum master")]
        public ActionResult EditSprints(int ProjectId = 0)
        {
             
            if (ProjectId > 0)
                HttpContext.Session["CurrentProject"] = ProjectId.ToString();
            else if (HttpContext.Session["CurrentProject"] == null)
                Redirect("/Project/Index");
            else
                ProjectId = Int32.Parse(HttpContext.Session["CurrentProject"].ToString());

            ViewBag.CurrentProject = ProjectId.ToString();

            Project project = db.Projects.Find(ProjectId);
            if (project == null)
            {
                return HttpNotFound();
            }

            return View(project);
        }


        [HttpPost]
        public ActionResult EditSprints(Project project)
        {
             
            //Project project = db.Projects.Find(ProjectId);
            Project prj = db.Projects.Find(project.ProjectId);

            if (prj != null)
            {
                prj.NumOfIterationDay = project.NumOfIterationDay;
                prj.WorkingDaysOnly = project.WorkingDaysOnly;


                //Clear previous sprints
                //clear from userstories
                List<UserStory> userstories = db.UserStories
                    .Where(us => us.Project_ProjectId == prj.ProjectId && us.Sprint.StartDate > DateTime.Today)
                    .ToList();

                foreach (UserStory us in userstories)
                {
                    us.Sprint = null;
                    us.Sprint_SprintId = null;
                    db.Entry(us).State = EntityState.Modified;
                }

                //Clear from project
                List<Sprint> sprintToremove = prj.Sprints.Where(s => s.StartDate > DateTime.Today).ToList();
                foreach (Sprint sp in sprintToremove)
                {
                    sp.Project = null;
                    sp.Project_ProjectId = 0;
                    sp.UserStories.Clear();
                    prj.Sprints.Remove(sp);
                    db.Sprints.Remove(sp);
                }

                DateTime startdate;
                Sprint cur = prj.Sprints.Where(sp => sp.EndDate > DateTime.Today).FirstOrDefault() ?? null;
                if (cur != null)
                {
                    startdate = cur.EndDate.AddDays(1);
                }
                else
                {
                    startdate = prj.StartDate;
                }

                //Add new sprints

                List<Sprint> sprints = getSprints(startdate,
                        prj.EndDate, prj.NumOfIterationDay, prj.WorkingDaysOnly);

                int nameCount = prj.Sprints.Count + 1;
                for (int i = 0; i < sprints.Count; i++)
                {
                    sprints[i].Name = "S" + nameCount++ + " " + sprints[i].StartDate.ToString("dd.MM.yy") + " - " + sprints[i].EndDate.ToString("dd.MM.yy");
                    sprints[i].Project_ProjectId = prj.ProjectId;
                    sprints[i].Project = prj;

                    prj.Sprints.Add(sprints[i]);
                    db.Sprints.Add(sprints[i]);
                }

                db.Entry(prj).State = EntityState.Modified;
                db.SaveChanges();

            }

            List<Sprint> nsprints = db.Sprints
                .Where(s => s.Project_ProjectId == prj.ProjectId)
                .OrderBy(s => s.SprintId).ToList();

            return PartialView("_SprintTable", nsprints);
        }


        [HttpPost]
        public ActionResult GetGraph(int ProjectId)
        {
             
            //{"name":"John"}
            GraphData graphData = new GraphData();

            Project prj = db.Projects.Find(ProjectId);
            int maxx = numberOfDays(prj.StartDate, prj.EndDate);
            int maxy = prj.UserStories.Select(s => s.Effort).Sum();
            int[][] tmpd2 = new int[maxx + 1][];
            int my = 0;

            tmpd2[0] = new int[2] { 0, maxy };

            for (int i = 1; i <= maxx; i++)
            {
                if (prj.StartDate.AddDays(i) > DateTime.Today) break;

                my = maxy - prj.UserStories.Where(us => us.CompletedDate <= prj.StartDate.AddDays(i))
                        .Select(s => s.Effort).Sum();

                tmpd2[i] = new int[2] { i, my };
            }
            
            graphData.maxx = maxx; //total days
            graphData.maxy = maxy; //total effort

            graphData.d2 = tmpd2; //total effort of work completed each day. [day,effort]
            return Json(graphData, JsonRequestBehavior.AllowGet);
        }


        //[HttpPost]
        //public ActionResult DrawChart(int ProjectId)
        //{            
        //    GraphData graphData = new GraphData();

        //    Project prj = db.Projects.Find(ProjectId);
        //    int maxx = db.UserStories.Where(us => us.Project_ProjectId == ProjectId).Count();
        //    int maxy = db.UserStories.Where(us => us.Project_ProjectId == ProjectId).Select(us => us.Effort).Max();
        //    int maxyu = db.UserStories.Where(us => us.Project_ProjectId == ProjectId).Select(us => us.User_Effort).Max();
            
                       
        //}



        public ActionResult TodaysTasks(int? page)
        {
             
            List<UserStory> userstories = db.UserStories
                .Where(us => us.UserProfile_UserId == WebMatrix.WebData.WebSecurity.CurrentUserId
                && us.CompletedDate == null && us.Sprint.EndDate >= DateTime.Today).ToList();

            int pageSize = 5;
            int pageNumber = (page ?? 1);
            var projectss = db.Projects.OrderBy(pr => pr.ProjectId);
            return PartialView(userstories.ToPagedList(pageNumber, pageSize));
        }

        [ChildActionOnly]
        [AllowAnonymous]
        public ActionResult MainMenu()
        {
             

            List<MenuItemModel> menu = new List<MenuItemModel>();

            if (WebMatrix.WebData.WebSecurity.IsAuthenticated)
            {     
                string projectid = "";
                try
                {
                    projectid = ControllerContext.HttpContext.Request.QueryString["ProjectId"].ToString();
                }
                catch (Exception ex)
                {
                    try
                    {
                        projectid = ControllerContext.ParentActionViewContext.RouteData.Values["id"].ToString();
                    }
                    catch (Exception ex2)
                    { }
                }
                if (Response.StatusCode == 404 || Response.StatusCode == 500 || 
                    (Request.RequestContext.RouteData.Values["Controller"].ToString() != "Project" || 
                    Request.RequestContext.RouteData.Values["Action"].ToString() != "Index"))
                {
                    menu.Add(new MenuItemModel { Text = "Начало", Action = "Index", Controller = "Project", Selected = false });
                }
                if (projectid.Length > 0)
                {
                    if (CheckProjectAuthentication(Convert.ToInt32(projectid)) || User.IsInRole("Admin"))
                    {
                        menu.Add(new MenuItemModel { Text = "Проект", Action = "Details", Controller = "Project", Selected = false, Routedata = new { id = projectid } });
                        menu.Add(new MenuItemModel { Text = "Backlog", Action = "index", Controller = "Backlog", Selected = false, Routedata = new { ProjectId = projectid } });
                        menu.Add(new MenuItemModel { Text = "Scrumboard", Action = "sprintboard", Controller = "Sprint", Selected = false, Routedata = new { ProjectId = projectid } });
                    }
                    else { menu.Add(new MenuItemModel { Text = "Начало", Action = "Index", Controller = "Project", Selected = false }); }
                }
            }
            return PartialView(menu);
        }

        public DateTime businessDaysFromStartDate(DateTime StartDate, int NumberOfBusinessDays)
        {

            //Knock the start date down one day if it is on a weekend.
            if (StartDate.DayOfWeek == DayOfWeek.Saturday |
                StartDate.DayOfWeek == DayOfWeek.Sunday)
            {
                NumberOfBusinessDays -= 1;
            }

            int index = 0;

            for (index = 1; index <= NumberOfBusinessDays; index++)
            {
                switch (StartDate.DayOfWeek)
                {
                    case DayOfWeek.Sunday:
                        StartDate = StartDate.AddDays(2);
                        break;
                    case DayOfWeek.Monday:
                    case DayOfWeek.Tuesday:
                    case DayOfWeek.Wednesday:
                    case DayOfWeek.Thursday:
                    case DayOfWeek.Friday:
                        StartDate = StartDate.AddDays(1);
                        break;
                    case DayOfWeek.Saturday:
                        StartDate = StartDate.AddDays(3);
                        break;
                }
            }

            //check to see if the end date is on a weekend.
            //If so move it ahead to Monday.
            //You could also bump it back to the Friday before if you desired to. 
            //Just change the code to -2 and -1.

            if (StartDate.DayOfWeek == DayOfWeek.Saturday)
            {
                StartDate = StartDate.AddDays(2);
            }
            else if (StartDate.DayOfWeek == DayOfWeek.Sunday)
            {
                StartDate = StartDate.AddDays(1);
            }

            return StartDate;
        }

        public int numberOfDays(DateTime Start, DateTime End)
        {
            TimeSpan ts;

            if (End > Start)
            { 
                ts = End - Start;
            }
            else
            { 
                ts = Start - End;
            }

            return ts.Days;
        }

        public System.DateTime addBusinessDays(System.DateTime StartDate, int NumberOfBusinessDays)
        {
            //Knock the start date down one day if it is on a weekend.
            if (StartDate.DayOfWeek == DayOfWeek.Saturday |
                StartDate.DayOfWeek == DayOfWeek.Sunday)
            {
                NumberOfBusinessDays -= 1;
            }

            int index = 0;

            for (index = 1; index <= NumberOfBusinessDays; index++)
            {
                switch (StartDate.DayOfWeek)
                {
                    case DayOfWeek.Sunday:
                        StartDate = StartDate.AddDays(2);
                        break;
                    case DayOfWeek.Monday:
                    case DayOfWeek.Tuesday:
                    case DayOfWeek.Wednesday:
                    case DayOfWeek.Thursday:
                    case DayOfWeek.Friday:
                        StartDate = StartDate.AddDays(1);
                        break;
                    case DayOfWeek.Saturday:
                        StartDate = StartDate.AddDays(3);
                        break;
                }
            }

            //check to see if the end date is on a weekend.
            //If so move it ahead to Monday.
            //You could also bump it back to the Friday before if you desired to. 
            //Just change the code to -2 and -1.

            if (StartDate.DayOfWeek == DayOfWeek.Saturday)
            {
                StartDate = StartDate.AddDays(2);
            }
            else if (StartDate.DayOfWeek == DayOfWeek.Sunday)
            {
                StartDate = StartDate.AddDays(1);
            }
            return StartDate;
        }

        public List<Sprint> getSprints(DateTime startDt, DateTime endDt, int interval, bool workingDaysOnly)
        {
            List<Sprint> sprints = new List<Sprint>();

            interval--;
            while (true)
            {
                Sprint s = new Sprint();
                s.StartDate = startDt;

                if (workingDaysOnly && addBusinessDays(startDt, interval) > endDt)
                {
                    s.EndDate = endDt;
                    sprints.Add(s);
                    break;
                }
                else if (!workingDaysOnly && startDt.AddDays(interval) > endDt)
                {
                    s.EndDate = endDt;
                    sprints.Add(s);
                    break;
                }
                else
                {
                    s.EndDate = workingDaysOnly ? addBusinessDays(startDt, interval) : startDt.AddDays(interval);
                }

                startDt = workingDaysOnly ? addBusinessDays(startDt, interval + 1) : startDt.AddDays(interval + 1);
                if (startDt <= endDt)
                    sprints.Add(s);
            }
            return sprints;
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }

    class GraphData
    {
        public int maxx { get; set; }
        public int maxy { get; set; }
        public int[][] d2 { get; set; }
    }
}