using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace StudentExerciseAPI.Model
{
    public class Exercise
    {
        
        public int Id { get; set; }
        [Required(ErrorMessage = "Exercise Name is required")]
        [StringLength(15, MinimumLength = 2, ErrorMessage = "Exercise Name should be between 2 and 15 characters")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Exercise Language is required")]
        [StringLength(15, MinimumLength = 2, ErrorMessage = "Exercise Language should be between 2 and 15 characters")]
        public string Language { get; set; }

        public List<Student> Students { get; set; } = new List<Student>();

    }
}
