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
    public class InstructorController : ControllerBase
    {
        private readonly IConfiguration _config;

        public InstructorController(IConfiguration config)
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
        public async Task<IActionResult> Get([FromQuery]string firstName, [FromQuery]string lastName, [FromQuery] string slackHandle)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT i.Id as InstructorId, i.FirstName, i.LastName, i.SlackHandle, i.Speciality, i.CohortId as InstructorCohortId, c.Id as CohortId, c.Name
                                        FROM Instructor i
                                        LEFT JOIN Cohort c on c.id = i.CohortId
                                        WHERE 1=1";
                    if (firstName != null)
                    {
                        cmd.CommandText += " AND i.FirstName LIKE @FirstName";
                        cmd.Parameters.Add(new SqlParameter(@"FirstName", firstName));
                    }

                    if (lastName != null)
                    {
                        cmd.CommandText += " AND i.LastName LIKE @LastName";
                        cmd.Parameters.Add(new SqlParameter(@"LastName", "%" + lastName + "%"));
                    }

                    if (slackHandle != null)
                    {
                        cmd.CommandText += " AND i.SlackHandle LIKE @SlackHandle";
                        cmd.Parameters.Add(new SqlParameter(@"SlackHandle", "%"+ slackHandle +"%"));
                    }
                    SqlDataReader reader = cmd.ExecuteReader();
                    List<Instructor> instructors = new List<Instructor>();

                    while (reader.Read())
                    {
                        var instructorId = reader.GetInt32(reader.GetOrdinal("InstructorId"));
                        Instructor instructorAlreadyAdded = instructors.FirstOrDefault(i => i.Id == instructorId);

                        if (instructorAlreadyAdded == null)
                        {

                            Instructor newInstructor = new Instructor
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("InstructorId")),
                                FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                                LastName = reader.GetString(reader.GetOrdinal("LastName")),
                                SlackHandle = reader.GetString(reader.GetOrdinal("SlackHandle")),
                                CohortId = reader.GetInt32(reader.GetOrdinal("InstructorCohortID")),
                                Specialty = reader.GetString(reader.GetOrdinal("Speciality")),
                              
                            };
                            instructors.Add(newInstructor);

                            var hasCohort = !reader.IsDBNull(reader.GetOrdinal("CohortId"));

                            if (hasCohort)
                            {
                                newInstructor.Cohort = new Cohort()
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("CohortId")),
                                    Name = reader.GetString(reader.GetOrdinal("Name"))

                                };
                            }
                        }else
                        {
                            var hasCohort = !reader.IsDBNull(reader.GetOrdinal("CohortId"));

                            if (hasCohort)
                            {
                                instructorAlreadyAdded.Cohort = new Cohort()
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("CohortId")),
                                    Name = reader.GetString(reader.GetOrdinal("Name"))

                                };
                            }
                        }
                       
                      
                    };
                    reader.Close();

                    return Ok(instructors);
                }
            }
        }

        [HttpGet("{id}", Name = "GetInstructor")]
        public async Task<IActionResult> Get([FromRoute] int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT s.Id, s.FirstName, s.LastName, s.SlackHandle,s.Speciality, s.CohortId as InstructorCohortId, c.Id as CohortId, c.Name
                        FROM Instructor s
                        LEFT JOIN Cohort c on c.id = s.CohortId
                        WHERE s.Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    Instructor instructor = null;

                    if (reader.Read())
                    {
                        int idColumnPosition = reader.GetOrdinal("CohortId");
                        int idValue = reader.GetInt32(idColumnPosition);

                        int nameColonPosition = reader.GetOrdinal("Name");
                        string cohortNameValue = reader.GetString(nameColonPosition);
                        instructor = new Instructor
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                            LastName = reader.GetString(reader.GetOrdinal("LastName")),
                            SlackHandle = reader.GetString(reader.GetOrdinal("SlackHandle")),
                            CohortId = reader.GetInt32(reader.GetOrdinal("InstructorCohortId")),
                            Specialty = reader.GetString(reader.GetOrdinal("Speciality")),
                            Cohort = new Cohort
                            {
                                Id = idValue,
                                Name = cohortNameValue
                            }
                        };


                    };
                    reader.Close();

                    if (instructor == null)
                    {
                        return NotFound($"No Instructor found with the id of {id}");
                    }
                    return Ok(instructor);
                }
            }
        }
        private bool InstructorExist(int id)
        {
            using SqlConnection conn = Connection;
            conn.Open();
            using (SqlCommand cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
                        SELECT Id, FirstName, LastName, SlackHandle, CohortId, Soeciality
                        FROM Instructor
                        WHERE Id = @id";
                cmd.Parameters.Add(new SqlParameter("@id", id));

                SqlDataReader reader = cmd.ExecuteReader();
                return reader.Read();
            }
        }
    }


}

