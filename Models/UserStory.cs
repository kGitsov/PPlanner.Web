using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using PagedList;

namespace PPlanner.Models
{
    public class UserStory
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(500)]
        public string Title { get; set; }
        public int BackLogPriority { get; set; }

        [Required]
        [Range(1,10)]
        public int Effort { get; set; }

        [Range(1, 10)]
        public int User_Effort { get; set; }

        public DateTime? CompletedDate { get; set; }

        public int? UserProfile_UserId { get; set; }
        public int Project_ProjectId { get; set; }
        public int ItemStatus_ItemStatusId { get; set; }
        public int? Sprint_SprintId { get; set; }

        [ForeignKey("Project_ProjectId")]
        public virtual Project Project { get; set; }
        
        [ForeignKey("ItemStatus_ItemStatusId")]
        public virtual ItemStatus ItemStatus { get; set; }
        
        [ForeignKey("Sprint_SprintId")]
        public virtual Sprint Sprint { get; set; }

        [ForeignKey("UserProfile_UserId")]
        public virtual UserProfile UserProfile { get; set; }

    }
}
