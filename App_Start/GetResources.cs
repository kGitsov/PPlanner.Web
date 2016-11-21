using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Web.WebPages.OAuth;
using PPlanner.Models;

namespace PPlanner
{
    public class GetResources
    {
        private static ProjectDbContext db = new ProjectDbContext();

        public static Dictionary<string, string> GetResourcesFromDB()
        {
            Dictionary<string, string> projectResources = new Dictionary<string, string>();
            var newVar = db.Resources.Select(res => new { res.ResName, res.ResValue });
            foreach (var item in newVar)
            {
                projectResources.Add(item.ResName, item.ResValue);
            }

            return projectResources;
        }




    }
}