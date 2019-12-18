using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StudentExerciseAPI.Model
{
    public class Exercise
    {
        /*
        public Exercise(int id, string name, string language)
        {
            Id = id;
            Name = name;
            Language = language;


        }
        */

        public int Id { get; set; }
        public string Name { get; set; }
        public string Language { get; set; }

        public List<Student> Students { get; set; } = new List<Student>();

    }
}
