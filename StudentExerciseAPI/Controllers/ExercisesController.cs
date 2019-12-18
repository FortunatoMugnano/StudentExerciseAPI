using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using StudentExerciseAPI.Model;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace StudentExerciseAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ExerciseController : ControllerBase
    {
        private readonly IConfiguration _config;

        public ExerciseController(IConfiguration config)
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
        public async Task<IActionResult> Get([FromQuery] string include, [FromQuery] string name, [FromQuery] string language)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT Exercise.Id as ExerciseID, Exercise.Name as ExerciseName, Exercise.Language as ExerciseLanguage";
                      if (include == "students")
                      {
                        cmd.CommandText += @", se.StudentId as StudentExerciseStudentID, se.exerciseId as StudentExerciseExerciseID, 
                        s.Id, s.FirstName, s.LastName, s.SlackHandle";
                        
                      }
                    cmd.CommandText += " FROM Exercise";
                      if (name != null)
                      {
                        cmd.CommandText += @" WHERE exercise.Name LIKE @name";
                        cmd.Parameters.Add(new SqlParameter("@name", "%" + name + "%"));
                      }
                      if (language != null)
                      {
                        cmd.CommandText += @" WHERE exercise.Language LIKE @language";
                        cmd.Parameters.Add(new SqlParameter("@language", "%" + language + "%"));
                      }
                        if (include == "students")
                        {
                          cmd.CommandText += @" LEFT JOIN Studentexercise as se ON Exercise.Id = se.exerciseId
                                              INNER JOIN Student as s ON se.studentId = s.Id
                                              WHERE 1=1";
                         
                        }
                    
                    
                    SqlDataReader reader = cmd.ExecuteReader();
                    List<Exercise> exercises = new List<Exercise>();

                    while (reader.Read())
                    {

                        int currentExerciseID = reader.GetInt32(reader.GetOrdinal("ExerciseID"));
                        Exercise newExercise = exercises.FirstOrDefault(e => e.Id == currentExerciseID);

                        string nameValue = reader.GetString(reader.GetOrdinal("ExerciseName"));
                        string languageValue = reader.GetString(reader.GetOrdinal("ExerciseLanguage"));

                        if (newExercise == null)
                        {
                            newExercise = new Exercise
                            { Id = currentExerciseID,
                            Name = nameValue, 
                            Language = languageValue
                            
                            
                            };
                            
                        }







                        exercises.Add(newExercise);

                        if (include == "students")
                        {

                            int currentStudentID = reader.GetInt32(reader.GetOrdinal("Id"));
                            foreach (Exercise exer in exercises)
                            {
                                if (exer.Id == reader.GetInt32(reader.GetOrdinal("StudentExerciseExerciseID")) && exer.Students.FirstOrDefault(s => s.Id == currentStudentID) == null)
                                {
                                    exer.Students.Add(new Student
                                    {
                                        Id = currentStudentID,
                                        FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                                        LastName = reader.GetString(reader.GetOrdinal("LastName")),
                                        SlackHandle = reader.GetString(reader.GetOrdinal("SlackHandle")),
                                    });
                                }
                            }
                        }
                    }

                   

                    reader.Close();

                    return Ok(exercises);
                }
            }
        }

        [HttpGet("{id}", Name = "GetExercise")]
        public async Task<IActionResult> Get([FromRoute] int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT
                            Id, Name, Language
                        FROM Exercise
                        WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    Exercise exercise = null;

                    if (reader.Read())
                    {
                        int idValue = reader.GetInt32(reader.GetOrdinal("Id"));
                        string nameValue = reader.GetString(reader.GetOrdinal("Name"));
                        string languageValue = reader.GetString(reader.GetOrdinal("Language"));

                        exercise = new Exercise
                        {
                            Id= idValue,
                            Name = nameValue,
                            Language = languageValue

                        };


                    };
                    reader.Close();

                    if (exercise == null)
                    {
                        return NotFound($"No Exercise found with the id of {id}");
                    }
                    return Ok(exercise);
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Exercise exercise)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO Exercise (Name, Language)
                                        OUTPUT INSERTED.Id
                                        VALUES (@name, @language)";
                    cmd.Parameters.Add(new SqlParameter("@name", exercise.Name));
                    cmd.Parameters.Add(new SqlParameter("@language", exercise.Language));
                    int newId = (int)cmd.ExecuteScalar();
                    exercise.Id = newId;
                    return CreatedAtRoute("GetExercise", new { id = newId }, exercise);
                }
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put([FromRoute] int id, [FromBody] Exercise exercise)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"UPDATE Exercise
                                            SET Name = @name, Language = @language
                                            WHERE Id = @id";
                        cmd.Parameters.Add(new SqlParameter("@name", exercise.Name));
                        cmd.Parameters.Add(new SqlParameter("@language", exercise.Language));
                        cmd.Parameters.Add(new SqlParameter("@id", id));

                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            return new StatusCodeResult(StatusCodes.Status204NoContent);
                        }
                        throw new Exception("No rows affected");
                    }
                }
            }
            catch (Exception)
            {
                if (!ExerciseExist(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"DELETE FROM Exercise WHERE Id = @id";
                        cmd.Parameters.Add(new SqlParameter("@id", id));

                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            return new StatusCodeResult(StatusCodes.Status204NoContent);
                        }
                        throw new Exception("No rows affected");
                    }
                }
            }
            catch (Exception)
            {
                if (!ExerciseExist(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        private bool ExerciseExist(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT Id, Name, Language 
                        FROM Exercise
                        WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = cmd.ExecuteReader();
                    return reader.Read();
                }
            }
        }
    }
}

