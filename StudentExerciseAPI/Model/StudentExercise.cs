using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StudentExerciseAPI.Model
{
    public class StudentExercise
    {
        /*
        public StudentExercise(int id, int studentId, int exerciseId)
        {
            Id = id;
            StudentId = studentId;
            ExerciseId = exerciseId;
        }
        */
        
        public int Id { get; set; }
        public int StudentId { get; set; }
        public int ExerciseId { get; set; }
    }
}
