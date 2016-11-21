using PPlanner.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace PPlanner.Web.Controllers
{
    public class ResourcesController : Controller
    {
        //
        // GET: /Resources/
        private ProjectDbContext db = new ProjectDbContext();

        public ActionResult UpdateEN()
        {
            return View();
        }
        public ActionResult Index(int? page)
        {
            Dictionary < int, Resources > projectResources = new Dictionary<int, Resources>();
            
            if (db.Resources.Count() > 0)
            {
                var newVar = db.Resources.Select(res => new { res.ResId, res.ResGroup, res.ResName, res.ResValue });
                foreach (var item in newVar)
                {
                    Resources temp = new Resources();
                    temp.ResGroup = item.ResGroup;
                    temp.ResName = item.ResName;
                    temp.ResValue = item.ResValue;
                    projectResources.Add(item.ResId, temp);
                }
            }
            else
            {
                Resources temp = new Resources();
                temp.ResGroup = "";
                temp.ResName = "";
                temp.ResValue = "";
                projectResources.Add(1, temp);
            }

            int currentPageIndex = page.HasValue ? page.Value - 1 : 0;
            //return View(projectResources.ToPagedList(currentPageIndex, 15));
            return View(projectResources);
        }

        public ActionResult Create()
        {
            IEnumerable<SelectListItem> GroupList = new SelectList(db.Resources.Select(gl => gl.ResGroup).Distinct(), "GroupList");
            ViewData["GroupList"] = GroupList;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Resources newResource)
        {
            if (ModelState.IsValid)
            {
                if (db.Resources.Where(res => res.ResName == newResource.ResName).FirstOrDefault() == null)
                {
                    db.Resources.Add(newResource);
                    db.SaveChanges();
                }                
            }
            return RedirectToAction("Index", "Resources");
        }

        public ActionResult Edit(int id)
        {
            Resources editRes = db.Resources.Where(res => res.ResId == id).FirstOrDefault();
            return View(editRes);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Resources resource)
        {
            if (ModelState.IsValid)
            {
                db.Entry(resource).State = EntityState.Modified;
                db.SaveChanges();
                
            }
            return RedirectToAction("Index", "Resources");
        }
        
        public FileResult DownloadFile()
        {
            var allRes = db.Resources.Select(ar => new { ar.ResGroup, ar.ResName, ar.ResValue });
            string filePath = Path.GetTempPath() + "test.csv";
            StringBuilder csvContent = new StringBuilder();
            csvContent.AppendLine("\uFEFF");
            foreach (var item in allRes)
            {
                csvContent.AppendLine(item.ResGroup + "," + item.ResName + "," + item.ResValue);
            }

            string fileName = "Resources.csv";

            return File(System.Text.Encoding.UTF8.GetBytes(csvContent.ToString()), System.Net.Mime.MediaTypeNames.Text.RichText, fileName);

        }

        public ActionResult Update()
        {
            return View();
        }

        
        [HttpPost]
        
        public ActionResult UpdateEN(HttpPostedFileBase fileUpload)
        {
            if (ModelState.IsValid)
            {
                if (fileUpload != null)
                {
                    Dictionary<string, string[]> newResources = new Dictionary<string, string[]>();

                    string line = "";

                    StreamReader reader = new StreamReader(fileUpload.InputStream, Encoding.UTF8);
                    while (reader.Peek() != -1)
                    {
                        line = reader.ReadLine();
                        if (line != "")
                        {
                            if (line.Contains("\t"))
                            {
                                string[] splitted = line.Split('\t');
                                newResources.Add(splitted[1], new string[] { splitted[0], splitted[2] });
                            }
                            else if (line.Contains(";"))
                            {
                                string[] splitted = line.Split(';');
                                newResources.Add(splitted[1], new string[] { splitted[0], splitted[2] });
                            }
                            else
                            {
                                string[] splitted = line.Split(',');
                                newResources.Add(splitted[1], new string[] { splitted[0], splitted[2] });
                            }
                        }
                    }
                    int id = 0;
                    foreach (var item in newResources)
                    {
                        if (db.Resources.Where(res => res.ResName == item.Key).FirstOrDefault() == null)
                        {
                            Resources temporaryResource = new Resources();
                            temporaryResource.ResId = id;
                            temporaryResource.ResGroup = item.Value[0];
                            temporaryResource.ResName =  item.Key;
                            temporaryResource.ResValue = item.Value[1];
                            id++;
                            
                            db.Resources.Add(temporaryResource);
                            db.SaveChanges();
                        }
                    }
                }
            }
            return RedirectToAction("Index", "Project");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Update(HttpPostedFileBase fileUpload)
        {
            if (ModelState.IsValid)
            {
                if (fileUpload != null)
                {
                    Dictionary<string, string[]> newResources = new Dictionary<string, string[]>();

                    string line = "";
                    
                    StreamReader reader = new StreamReader(fileUpload.InputStream,Encoding.UTF8);
                    while (reader.Peek() != -1)
                    {
                        line = reader.ReadLine();
                        if (line != "")
                        {
                            if (line.Contains("\t"))
                            {
                                string[] splitted = line.Split('\t');
                                newResources.Add(splitted[1], new string[] { splitted[0], splitted[2] });
                            }
                            else if (line.Contains(";"))
                            {
                                string[] splitted = line.Split(';');
                                newResources.Add(splitted[1], new string[] { splitted[0], splitted[2] });
                            }
                            else
                            {
                                string[] splitted = line.Split(',');
                                newResources.Add(splitted[1], new string[] { splitted[0], splitted[2] });
                            }
                        }
                        
                    }
                    foreach (var item in newResources)
                    {
                        //int id = db.Resources.Where(res => res.ResName == item.Key).FirstOrDefault().ResId;
                        //string group = db.Resources.Where(res => res.ResName == item.Key).FirstOrDefault().ResGroup;                                               

                        if (db.Resources.Where(res => res.ResName == item.Key).FirstOrDefault() != null)
                        {
                            Resources temporaryResource = new Resources();

                            //temporaryResource.ResId = id;
                            temporaryResource.ResGroup = item.Value[0];
                            temporaryResource.ResName =  item.Key;
                            temporaryResource.ResValue = item.Value[1];

                            if (db.Resources.Where(dr => dr.ResName == temporaryResource.ResName).Count() > 0)
                            {
                                int id = db.Resources.Where(res => res.ResName == temporaryResource.ResName).FirstOrDefault().ResId;
                                var v = db.Resources.Find(id);
                                db.Entry(v).CurrentValues.SetValues(temporaryResource);
                            }
                            else
                            {
                                db.Resources.Add(temporaryResource);
                            }

                            db.SaveChanges();
                        }                        
                    }                    
                }
            }
            return View();
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}
