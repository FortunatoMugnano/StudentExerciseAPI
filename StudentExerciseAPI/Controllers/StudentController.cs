using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using StudentExerciseAPI.Model;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace StudentExerciseAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StudentController : ControllerBase
    {
        private readonly IConfiguration _config;

        public StudentController(IConfiguration config)
        {
            _config = config;
        }

        public SqlConnection Connection
        {
            get
            {
                return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            }
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery]string include, [FromQuery]string firstName, [FromQuery]string lastName, [FromQuery] string slackHandle)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {   
                    
                    
                        
                            cmd.CommandText += @"SELECT s.Id, s.FirstName, s.LastName, s.SlackHandle, s.CohortId, c.Name AS CohortName,
                                                 e.Id AS ExerciseId, e.Language, e.Name
                                                 FROM Student s
                                                 INNER JOIN Cohort c On s.CohortId = c.Id
                                                 INNER JOIN StudentExercise se ON se.StudentId = s.Id 
                                                 INNER JOIN Exercise e ON se.ExerciseId = e.Id
                                                 WHERE 1=1
                                                 ";
                            if (firstName != null)
                            {
                                cmd.CommandText += " AND s.FirstName LIKE @FirstName";
                                cmd.Parameters.Add(new SqlParameter(@"FirstName", firstName));
                            }

                            if (lastName != null)
                            {
                                cmd.CommandText += " AND s.LastName LIKE @LastName";
                                cmd.Parameters.Add(new SqlParameter(@"LastName", "%" + lastName + "%"));
                            }

                            if (slackHandle != null)
                            {
                                cmd.CommandText += " AND s.SlackHandle LIKE @SlackHandle";
                                cmd.Parameters.Add(new SqlParameter(@"SlackHandle", "%" + slackHandle + "%"));
                            }

                        
                   
                 
                    SqlDataReader reader = cmd.ExecuteReader();
                    List<Student> students = new List<Student>();

                    while (reader.Read())
                    {
                        var studentId = reader.GetInt32(reader.GetOrdinal("Id"));
                        //search to see if student is already added
                        var studentAlreadyAdded = students.FirstOrDefault(s => s.Id == studentId);
                        //create bool for if there is an exercise in row
                        var hasExercise = !reader.IsDBNull(reader.GetOrdinal("ExerciseId"));
                        //if statement for adding new student, null means they were NOT found, let's add them!
                        if (studentAlreadyAdded == null)
                        {

                            Student student = new Student
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                CohortId = reader.GetInt32(reader.GetOrdinal("CohortId")),
                                LastName = reader.GetString(reader.GetOrdinal("LastName")),
                                FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                                SlackHandle = reader.GetString(reader.GetOrdinal("SlackHandle")),
                                Exercises = new List<Exercise>(),

                            };
                            students.Add(student);

                            var hasCohort = !reader.IsDBNull(reader.GetOrdinal("CohortId"));

                            if (hasCohort)
                            {
                                student.Cohort = new Cohort()
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("CohortId")),
                                    Name = reader.GetString(reader.GetOrdinal("CohortName"))

                                };
                            }

                            //If row has an exercise AND the query param "include" = exercises, then add it to the exercise list
                            if (hasExercise && include == "exercises")
                            {
                                student.Exercises.Add(new Exercise()
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("ExerciseId")),
                                    Name = reader.GetString(reader.GetOrdinal("Name")),
                                    Language = reader.GetString(reader.GetOrdinal("Language"))
                                });

                            }
                        }
                        else
                        //Student was already added!  Lets check to see if there are exercises to add and assign a Cohort
                        {
                            var hasCohort = !reader.IsDBNull(reader.GetOrdinal("CohortId"));

                            if (hasCohort)
                            {
                                studentAlreadyAdded.Cohort = new Cohort()
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("CohortId")),
                                    Name = reader.GetString(reader.GetOrdinal("CohortName"))

                                };
                            }

                            if (hasExercise && include == "exercises")
                            {
                                studentAlreadyAdded.Exercises.Add(new Exercise()
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("ExerciseId")),
                                    Name = reader.GetString(reader.GetOrdinal("Name")),
                                    Language = reader.GetString(reader.GetOrdinal("Language"))
                                });

                            }
                        }
                    }
                    reader.Close();
                   
                    return Ok(students);

                 
                }
            }
        }

        [HttpGet("{id}", Name = "GetStudent")]
        public async Task<IActionResult> Get([FromRoute] int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT s.Id, s.FirstName, s.LastName, s.SlackHandle, s.CohortId as StudentCohortId, c.Id as CohortId, c.Name
                        FROM Student s
                        LEFT JOIN Cohort c on c.id = s.CohortId
                        WHERE s.Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    Student student = null;

                    if (reader.Read())
                    {
                        int idColumnPosition = reader.GetOrdinal("CohortId");
                        int idValue = reader.GetInt32(idColumnPosition);

                        int nameColonPosition = reader.GetOrdinal("Name");
                        string cohortNameValue = reader.GetString(nameColonPosition);
                        student = new Student
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                            LastName = reader.GetString(reader.GetOrdinal("LastName")),
                            SlackHandle = reader.GetString(reader.GetOrdinal("SlackHandle")),
                            CohortId = reader.GetInt32(reader.GetOrdinal("StudentCohortId")),
                            Cohort = new Cohort
                            {
                                Id = idValue,
                                Name = cohortNameValue
                            }
                        };


                    };
                    reader.Close();

                    if (student == null)
                    {
                        return NotFound($"No Student found with the id of {id}");
                    }
                    return Ok(student);
                }
            }
        }
        
        /*
        [HttpGet]
        [Route("studentCohort")]
        public async Task<IActionResult> Get([FromQuery]int? cohortId, [FromQuery]string lastName)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT s.Id, s.FirstName, s.LastName, s.SlackHandle, s.CohortId as StudentCohortId, c.Id as CohortId, c.Name
                                        FROM Student s
                                        LEFT JOIN Cohort c on c.id = s.CohortId
                                        WHERE 1=1";
                    if (cohortId != null)
                    {
                        cmd.CommandText += " AND CohortId = @cohortId";
                        cmd.Parameters.Add(new SqlParameter("@cohortId", cohortId));
                    }
                    if (lastName != null)
                    {
                        cmd.CommandText += " AND LastName LIKE @lastName";
                        cmd.Parameters.Add(new SqlParameter("@lastName", "%" + lastName + "%"));
                    }
                    SqlDataReader reader = cmd.ExecuteReader();
                    List<Student> allStudents = new List<Student>();
                    while (reader.Read())
                    {
                        Student stu = new Student
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                            LastName = reader.GetString(reader.GetOrdinal("LastName")),
                            SlackHandle = reader.GetString(reader.GetOrdinal("SlackHandle")),
                            CohortId = reader.GetInt32(reader.GetOrdinal("CohortId"))
                        };
                        allStudents.Add(stu);
                    }
                    reader.Close();
                    return Ok(allStudents);
                }
            }
        }

       */

        private bool StudentExist(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT Id, FirstName, LastName, SlackHandle, CohortId 
                        FROM Student
                        WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = cmd.ExecuteReader();
                    return reader.Read();
                }
            }
        }
    }

    

}
