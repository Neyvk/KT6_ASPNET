using KT6_Neyaskin.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.IO;

namespace KT6_Neyaskin.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IConfiguration _configuration;
            
        public UsersController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPut("update")]
        public IActionResult UpdateUser(int id, User user)
        {
            string connectionString =
                _configuration.GetConnectionString("DefaultConnection");

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                SqlTransaction tr = connection.BeginTransaction();

                try
                {
                    SqlCommand command = connection.CreateCommand();
                    command.Transaction = tr;

                    command.CommandText = @"UPDATE Users
                                    SET Name = @Name,
                                        Email = @Email,
                                        Age = @Age
                                    WHERE Id = @Id";

                    command.Parameters.AddWithValue("@Id", id);
                    command.Parameters.AddWithValue("@Name", user.Name);
                    command.Parameters.AddWithValue("@Email", user.Email);
                    command.Parameters.AddWithValue("@Age", user.Age);

                    int result = command.ExecuteNonQuery();

                    if (result == 0)
                    {
                        tr.Rollback();
                        Log("ERROR not found id=" + id);
                        return BadRequest("Пользователь не найден");
                    }

                    tr.Commit();

                    Log("SUCCESS update id=" + id);

                    return Ok("Успешно обновлено");
                }
                catch (Exception ex)
                {
                    tr.Rollback();
                    Log("ERROR id=" + id + " " + ex.Message);
                    return BadRequest("Ошибка");
                }
            }
        }
        [HttpPut("updateemail")]
        public IActionResult UpdateEmailIfNameMatches(int id, string name, string email)
        {
            string connectionString =
                _configuration.GetConnectionString("DefaultConnection");

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string sql = @"UPDATE Users 
                       SET Email=@Email 
                       WHERE Id=@Id AND Name=@Name";

                SqlCommand cmd = new SqlCommand(sql, conn);

                cmd.Parameters.AddWithValue("@Id", id);
                cmd.Parameters.AddWithValue("@Name", name);
                cmd.Parameters.AddWithValue("@Email", email);

                conn.Open();
                int result = cmd.ExecuteNonQuery();

                if (result < 1)
                {
                    Log("ERROR id=" + id);
                    return BadRequest("Имя не совпало или пользователь не найден");
                }
                Log("UpdateUser OK id=" + id);
                return Ok("Email обновлён");


            }
        }
        private void Log(string message)
        {
            var path = Path.Combine(AppContext.BaseDirectory, "log.txt");

            System.IO.File.AppendAllText(
                path,
                $"{DateTime.Now} {message}{Environment.NewLine}"
            );
        }
    }
}
