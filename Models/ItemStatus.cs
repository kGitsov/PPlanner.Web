using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace PPlanner.Models
{
    public class ItemStatus
    {
        //public int ItemStatusId { get; set; }

        //[StringLength(100)]
        //public string Name { get; set; }


        public ItemStatus()
        {
            this.UserStories = new HashSet<UserStory>();
        }
    
        public int ItemStatusId { get; set; }

        [StringLength(100)]
        public string Status { get; set; }
    
        public virtual ICollection<UserStory> UserStories { get; set; }

    }
}