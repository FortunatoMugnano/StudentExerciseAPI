using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace StudentExerciseAPI.Model
{
    public class Student
    {

        public int Id { get; set; }
        [Required(ErrorMessage = "Student First Name is required")]
        [StringLength(15, MinimumLength = 2, ErrorMessage = "Student First Name should be between 2 and 15 characters")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Student Last Name is required")]
        [StringLength(15, MinimumLength = 2, ErrorMessage = "Student Last Name should be between 2 and 15 characters")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Student SlackHandle is required")]
        [StringLength(12, MinimumLength = 3, ErrorMessage = "Student SlackHandle should be between 3 and 12 characters")]
        public string SlackHandle { get; set; }
        public Cohort Cohort { get; set; }

        public int CohortId { get; set; }


        public List<Exercise> Exercises { get; set; } = new List<Exercise>();
    }
}

