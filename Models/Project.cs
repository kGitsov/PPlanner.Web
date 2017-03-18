using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace PPlanner.Models
{

    public class Project : IValidatableObject
    {

        public Project()
        {
            this.UserStories = new HashSet<UserStory>();
            this.Sprints = new HashSet<Sprint>();
        }

        [Key]
        public int ProjectId { get; set; }

        [Required(ErrorMessage ="Задължително поле.")]
        [StringLength(300, ErrorMessage = "Въведете стойност до 300 символа.")]
        [Display(Name = "Име на проект")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Задължително поле")]
        [Display(Name = "Дата от")]
        [DataType(DataType.Date, ErrorMessage = "Задължително поле")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", HtmlEncode = false)]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage ="Задължително поле")]
        [Display(Name = "Дата до")]
        [DataType(DataType.Date, ErrorMessage = "Задължително поле") ]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", HtmlEncode = false)]
        public DateTime EndDate { get; set; }

        [Required(ErrorMessage ="Задължително поле")]
        [Display(Name="Общо дни")]
        [Range(2,60)]
        public int NumOfIterationDay { get; set; }

        [Required]
        [Display(Name = "Само работни дни")]
        
        public bool WorkingDaysOnly { get; set; }

        [Display(Name = "SM_UserId")]
        public int SM_UserId { get; set; }

        public int CurrentSprint_SprintId { get; set; }

        public virtual ICollection<UserStory> UserStories { get; set; }
        public virtual ICollection<Sprint> Sprints { get; set; }



        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (EndDate <= StartDate)
            {
                yield return new ValidationResult("Крайната дата, трябва да бъде по-голяма от началната.");
            }
        }

        //public Dictionary<string, string> resources { get; set; }
    }
}