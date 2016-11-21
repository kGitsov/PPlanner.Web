using PPlanner.Web;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Globalization;
using System.Security.AccessControl;
using System.Web.Security;

namespace PPlanner.Models
{
    [Table("ProjectDevelopers")]
    public class ProjectDevelopers
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required]
        public int DevId { get; set; }
        [Required]
        public int ProjectId { get; set; }

        public string PositionName { get; set; }

    }
}