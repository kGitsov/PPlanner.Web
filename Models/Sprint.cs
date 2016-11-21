using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace PPlanner.Models
{
    public class Sprint
    {

        public Sprint()
        {
            this.UserStories = new HashSet<UserStory>();
        }

        [Key]
        public int SprintId { get; set; }
        
        [Required]
        [Display(Name = "Sprint name")]
        public string Name { get; set; }

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        [Display(Name="Start date")]
        public DateTime StartDate { get; set; }

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        [Display(Name = "End date")]
        public DateTime EndDate { get; set; }

        public bool IsCurrent { get; set; }

        public int Project_ProjectId { get; set; }

        public virtual ICollection<UserStory> UserStories { get; set; }

        [ForeignKey("Project_ProjectId")]
        public virtual Project Project { get; set; }


    }
}