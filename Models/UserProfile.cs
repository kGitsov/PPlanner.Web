using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace PPlanner.Models
{
    [Table("UserProfile")]
    public class UserProfile
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int UserId { get; set; }

        [MaxLength(30)]
        public string UserName { get; set; }

        public bool isApproved { get; set; }

        [Display(Name = "First name")]
        public string FirstName { get; set; }
        [Display(Name = "Last name")]
        public string LastName { get; set; }

        [Display(Name = "Position")]
        public string Position { get; set; }

        [Display(Name = "RoleName")]
        public string RoleName { get; set; }

        [DataType(DataType.EmailAddress)]
        [Display(Name ="Email Address")]
        public string Email { get; set; }

        [Column(TypeName = "Image")]
        public byte[] Image { get; set; }
                
        public virtual ICollection<UserStory> UserStories { get; set; }
    }    
}