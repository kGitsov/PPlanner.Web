using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace PPlanner.Models
{
    public class Release
    {
        //[Key]
        //public int ReleaseId { get; set; }
        //public int ReleaseNumber { get; set; }

        //public DateTime StartDate { get; set; }
        //public DateTime EndDate { get; set; }

        //public Project Project { get; set; }
        //public List<Sprint> Sprints { get; set; }


        public Release()
        {
            this.Sprints = new HashSet<Sprint>();
        }

        [Key]
        public int ReleaseId { get; set; }
        public int ReleaseNumber { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        
        //public int ProjectId { get; set; }

        public virtual ICollection<Sprint> Sprints { get; set; }
        public virtual Project Project { get; set; }

    }
}