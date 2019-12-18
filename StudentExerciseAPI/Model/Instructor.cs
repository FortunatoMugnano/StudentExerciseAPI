using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StudentExerciseAPI.Model
{
    public class Instructor
    {
       /* public Instructor(int id, string firstName, string lastName, string slackHandle, string speciality, int cohortId, Cohort cohort)
        {
            Id = id;
            FirstName = firstName;
            LastName = lastName;
            SlackHandle = slackHandle;
            Specialty = speciality;
            CohortId = cohortId;
            Cohort = cohort;
            }

    */


            
        
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string SlackHandle { get; set; }
        public string Specialty { get; set; }
        public int CohortId { get; set; }
        public Cohort Cohort { get; set; } 

    }
}
