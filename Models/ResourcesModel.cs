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
    [Table("Resources")]
    public class Resources
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int ResId { get; set; }
        [Required]
        [MaxLength(100)]
        public string ResGroup { get; set; }
        [Required]
        [MaxLength(100)]
        public string ResName { get; set; }
        [MaxLength(1000)]
        public string ResValue { get; set; }


    }
}