using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace PPlanner.Models
{
    public class LanguageModels : IValidatableObject
    {
        public LanguageModels()
        {

        }

        [Key]
        public int IId { get; set; }

        [Required]
        [StringLength(300)]
        [Display(Name = "Име на проект")]
        public string Name { get; set; }


        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            throw new NotImplementedException();
        }
    }
}
