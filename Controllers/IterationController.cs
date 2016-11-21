using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PPlanner.Models;

namespace PPlanner.Controllers
{
        [Authorize]
    public class IterationController : Controller
    {
        private ProjectDbContext db = new ProjectDbContext();

        //
        // GET: /Iteration/

        public ActionResult Index(int ProjectId = 0)
        {

            if (ProjectId > 0)
                HttpContext.Session["CurrentProject"] = ProjectId.ToString();
            else if (HttpContext.Session["CurrentProject"] == null)
                Redirect("/Project/Index");
            else
                ProjectId = Int32.Parse(HttpContext.Session["CurrentProject"].ToString());

            ViewBag.CurrentProject = ProjectId.ToString();
            
            var sprints = db.Sprints
                            .Where(s => s.Project_ProjectId == ProjectId)
                            .OrderBy(s => s.StartDate);
                        //.Include(s => s.Project);

            return View(sprints.ToList());
        }

        //
        // GET: /Iteration/Details/5
        //public ActionResult Details(string name )
        //{
        //    Sprint sprint = db.Sprints.Where(sp => sp.Name == name).FirstOrDefault();
        //    if (sprint == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(sprint);
        //}

        ////
        //// GET: /Iteration/Create

        //public ActionResult Create()
        //{

        //    return View();
        //}

        ////
        //// POST: /Iteration/Create

        //[HttpPost]
        //public ActionResult Create(Sprint sprint)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        db.Sprints.Add(sprint);
        //        db.SaveChanges();
        //        return RedirectToAction("Index");
        //    }

        //    return View(sprint);
        //}

        //
        // GET: /Iteration/Edit/5

        public ActionResult Edit(int id = 0)
        {
            Sprint sprint = db.Sprints.Find(id);
            if (sprint == null)
            {
                return HttpNotFound();
            }

            return View(sprint);
        }

        //
        // POST: /Iteration/Edit/5

        [HttpPost]
        public ActionResult Edit(Sprint sprint)
        {
            if (ModelState.IsValid)
            {
                db.Entry(sprint).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(sprint);
        }

        //
        // GET: /Iteration/Delete/5

        public ActionResult Delete(int id = 0)
        {
            Sprint sprint = db.Sprints.Find(id);
            if (sprint == null)
            {
                return HttpNotFound();
            }
            return View(sprint);
        }

        //
        // POST: /Iteration/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            Sprint sprint = db.Sprints.Find(id);
            db.Sprints.Remove(sprint);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult SprintTable(int ProjectId, int NumOfIterationDay, bool WorkingDaysOnly)
        {

            Project project = db.Projects.Find(ProjectId);

            if (project != null)
            {
                project.NumOfIterationDay = NumOfIterationDay;
                project.WorkingDaysOnly = WorkingDaysOnly;

                List<UserStory> userstories = db.UserStories
                    .Where(us => us.Project_ProjectId == ProjectId)
                    .ToList();

                foreach (UserStory us in userstories)
                {
                    us.Sprint = null;
                    us.Sprint_SprintId = 0;

                    db.Entry(us).State = EntityState.Modified;
                }

                List<Sprint> sprnt = project.Sprints.ToList();  
                project.Sprints.Clear();  
                foreach (Sprint sp in sprnt)
	             {
                    sp.Project = null;
                    sp.Project_ProjectId = 0;
                    sp.UserStories.Clear();
                    db.Sprints.Remove(sp);
	             }
                

                List<Sprint> sprints = getSprints(project.StartDate,
                        project.EndDate, project.NumOfIterationDay, project.WorkingDaysOnly);

                for (int i = 0; i < sprints.Count; i++)
                {
                    sprints[i].Name = "S" + (i + 1) + " " + sprints[i].StartDate.ToString("dd.MM.yy") + " - " + sprints[i].EndDate.ToString("dd.MM.yy");
                    sprints[i].Project_ProjectId = project.ProjectId;
                    sprints[i].Project = project;

                    project.Sprints.Add(sprints[i]);
                    db.Sprints.Add(sprints[i]);
                }

                foreach (UserStory us in userstories)
                {
                    
                    us.Sprint = sprints[0];
                    us.Sprint_SprintId = sprints[0].SprintId;
                    sprints[0].UserStories.Add(us);

                    db.Entry(us).State = EntityState.Modified;
                }

                db.Entry(project).State = EntityState.Modified;

                db.SaveChanges();

            }



            List<Sprint> nsprints = db.Sprints
                .Where(s => s.Project_ProjectId == ProjectId)
                .OrderBy(s => s.SprintId).ToList();

            return PartialView("_SprintTable", nsprints);

        }

        public ActionResult SprintList(int ProjectId)
        {
            List<Sprint> sprints = db.Sprints
                .Where(s => s.Project_ProjectId == ProjectId)
                .OrderBy(s => s.SprintId).ToList();

            return PartialView("_SprintList", sprints);
        }


        public int numberOfDays(DateTime Start, DateTime End)
        {
            TimeSpan ts;
            if (End > Start)
                ts = End - Start;
            else
                ts = Start - End;

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
}