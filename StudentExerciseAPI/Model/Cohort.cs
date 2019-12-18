using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace StudentExerciseAPI.Model
{
    public class Cohort
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Cohort Name is required")]
        [StringLength(11, MinimumLength = 5, ErrorMessage = "Cohort Name should be between 5 and 11 characters")]
        public string Name { get; set; }

        public List<Student> Students { get; set; } = new List<Student>();

        public List<Instructor> Instructors { get; set; } = new List<Instructor>();

    }
}
