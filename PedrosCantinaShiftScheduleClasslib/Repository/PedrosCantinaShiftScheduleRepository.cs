using DotNetEnv;
using Microsoft.Data.SqlClient;

namespace PedrosCantinaShiftScheduleClasslib;

public class PedrosCantinaShiftScheduleRepository
{
    private static string _connectionString;
    private SqlConnection _connection;

    public PedrosCantinaShiftScheduleRepository()
    {
        Env.Load();
        try
        {
            _connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");
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

    private const string _sql_createEmployee = "INSERT INTO dbo.Employee (Name, Phone, Email) VALUES (@name, @phone, @email)";
    public void CreateEmployee(string name, string phone, string email)
    {
        OpenConnection();
        SqlCommand command = new SqlCommand(_sql_createEmployee, _connection);
        command.Parameters.AddWithValue("@name", name);
        command.Parameters.AddWithValue("@phone", phone);
        command.Parameters.AddWithValue("@email", email);

        int rowsAffected = command.ExecuteNonQuery();
        if (rowsAffected > 0)
        {
            Console.WriteLine("Employee created.");
        }
        else
        {
            Console.WriteLine("Employee could not be created.");
        }

        CloseConnection();
    }

    private const string _sql_getAllEmployees = "SELECT Id, Name, Phone, Email FROM dbo.Employee";
    public void GetAllEmployees()
    {
        OpenConnection();
        SqlCommand command = new SqlCommand(_sql_getAllEmployees, _connection);

        SqlDataReader reader = command.ExecuteReader();
        if (reader.HasRows)
        {
            while (reader.Read())
            {
                int id = reader.GetInt32(0);
                string name = reader.GetString(1);
                string phone = reader.GetString(2);
                string email = reader.GetString(3);

                Console.WriteLine($"Id: {id}, Name: {name}, Phone: {phone}, Email: {email}");
            }
        }
        else
        {
            Console.WriteLine("No employees found.");
        }

        reader.Close();
        CloseConnection();
    }

    private const string _sql_employeeExists = "SELECT COUNT(*) FROM dbo.Employee WHERE Id = @employeeId";
    public bool EmployeeExists(int employeeId)
    {
        OpenConnection();
        SqlCommand command = new SqlCommand(_sql_employeeExists, _connection);
        command.Parameters.AddWithValue("@employeeId", employeeId);

        int employeeCount = Convert.ToInt32(command.ExecuteScalar());
        CloseConnection();

        return employeeCount > 0;
    }

    private const string _sql_updateEmployee = "UPDATE dbo.Employee SET Name = @name, Phone = @phone, Email = @email WHERE Id = @employeeId";
    public void UpdateEmployee(int employeeId, string name, string phone, string email)
    {
        OpenConnection();
        SqlCommand command = new SqlCommand(_sql_updateEmployee, _connection);
        command.Parameters.AddWithValue("@employeeId", employeeId);
        command.Parameters.AddWithValue("@name", name);
        command.Parameters.AddWithValue("@phone", phone);
        command.Parameters.AddWithValue("@email", email);

        int rowsAffected = command.ExecuteNonQuery();
        if (rowsAffected > 0)
        {
            Console.WriteLine("Employee updated.");
        }
        else
        {
            Console.WriteLine("Employee not found.");
        }

        CloseConnection();
    }

    private const string _sql_deleteEmployee = "DELETE FROM dbo.Employee WHERE Id = @employeeId";
    public void DeleteEmployee(int employeeId)
    {
        OpenConnection();
        SqlCommand command = new SqlCommand(_sql_deleteEmployee, _connection);
        command.Parameters.AddWithValue("@employeeId", employeeId);

        int rowsAffected = command.ExecuteNonQuery();
        if (rowsAffected > 0)
        {
            Console.WriteLine("Employee deleted.");
        }
        else
        {
            Console.WriteLine("Employee not found.");
        }

        CloseConnection();
    }
}
