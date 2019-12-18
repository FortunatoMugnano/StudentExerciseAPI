using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StudentExerciseAPI.Model
{
    public class Cohort
    {
        /*
        public Cohort(int id, string name)
        {
            Id = id;
            Name = name;
        }
        */
        public int Id { get; set; }
        public string Name { get; set; }

        public List<Student> Students { get; set; } = new List<Student>();

        public List<Instructor> Instructors { get; set; } = new List<Instructor>();

    }
}
