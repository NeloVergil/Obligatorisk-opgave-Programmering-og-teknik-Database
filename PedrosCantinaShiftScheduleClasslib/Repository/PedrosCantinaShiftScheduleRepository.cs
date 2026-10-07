using Microsoft.Data.SqlClient;
namespace PedrosCantinaShiftScheduleClasslib;

public class PedrosCantinaShiftScheduleRepository
{
    private const string _connectionString = @"Data Source=localhost;Initial Catalog=PedrosCantinaShiftScheduleDB;User ID=sa;Password=@dmin123;Pooling=False;Connect Timeout=30;Encrypt=True;Trust Server Certificate=True;Authentication=SqlPassword;Application Name=vscode-mssql;Application Intent=ReadWrite;Command Timeout=30";
    private SqlConnection _connection;

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
