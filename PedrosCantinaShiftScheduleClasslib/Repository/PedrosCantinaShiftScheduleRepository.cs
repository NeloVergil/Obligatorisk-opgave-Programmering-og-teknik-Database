using DotNetEnv;
using Microsoft.Data.SqlClient;

namespace PedrosCantinaShiftScheduleClasslib;

public class PedrosCantinaShiftScheduleRepository
{
    private static readonly string _connectionString = LoadConnectionString();
    private SqlConnection _connection;

    private static string LoadConnectionString()
    {
        Env.Load();
        try
        {
            return Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to load connection string.", ex);
        }
    }

    private void OpenConnection()
    {
        _connection = new SqlConnection(_connectionString);
        _connection.Open();
    }

    private void CloseConnection()
    {
        _connection.Close();
    }

    private const string _sql_getEmployeeContactInfo = "SELECT Name, Phone, Email FROM dbo.Employee WHERE Id = @employeeId";
    public void GetEmployeeContactInfo(int employeeId)
    {
        OpenConnection();
        SqlCommand command = new SqlCommand(_sql_getEmployeeContactInfo, _connection);
        command.Parameters.AddWithValue("@employeeId", employeeId);

        SqlDataReader reader = command.ExecuteReader();
        if (reader.Read())
        {
            string name = reader.GetString(0);
            string phone = reader.GetString(1);
            string email = reader.GetString(2);

            Console.WriteLine($"Name: {name}, Phone: {phone}, Email: {email}");
        }
        else
        {
            Console.WriteLine("Employee not found.");
        }

        reader.Close();
        CloseConnection();
    }
}
