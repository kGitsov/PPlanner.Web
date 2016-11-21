using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PPlanner.Models
{
    public class MenuItemModel
    {


        public string Text { get; set; }
        public string Controller { get; set; }
        public string Action { get; set; }
        public bool Selected { get; set; }
        public Object Routedata { get; set; }


    }
}