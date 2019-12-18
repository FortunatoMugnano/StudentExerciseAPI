using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace StudentExerciseAPI.Model
{
    public class Instructor
    {
                  
        public int Id { get; set; }
        [Required(ErrorMessage = "Instructor First Name is required")]
        [StringLength(15, MinimumLength = 2, ErrorMessage = "Instructor First Name should be between 2 and 15 characters")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Instructor Last Name is required")]
        [StringLength(15, MinimumLength = 2, ErrorMessage = "Instructor Last Name should be between 2 and 15 characters")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Instructor SlackHandle is required")]
        [StringLength(12, MinimumLength = 3, ErrorMessage = "Instructor SlackHandle should be between 3 and 12 characters")]
        public string SlackHandle { get; set; }

        public string Specialty { get; set; }
        public int CohortId { get; set; }
        public Cohort Cohort { get; set; } 

    }
}
