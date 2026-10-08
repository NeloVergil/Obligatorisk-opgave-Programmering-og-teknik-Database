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

    private const string _sql_deleteEmployeeAssignments = "DELETE FROM dbo.ShiftEmployee WHERE EmployeeId = @employeeId";
    private const string _sql_deleteEmployee = "DELETE FROM dbo.Employee WHERE Id = @employeeId";
    public void DeleteEmployee(int employeeId)
    {
        OpenConnection();
        SqlTransaction transaction = _connection.BeginTransaction();

        try
        {
            SqlCommand deleteAssignmentsCommand = new SqlCommand(_sql_deleteEmployeeAssignments, _connection, transaction);
            deleteAssignmentsCommand.Parameters.AddWithValue("@employeeId", employeeId);
            deleteAssignmentsCommand.ExecuteNonQuery();

            SqlCommand deleteEmployeeCommand = new SqlCommand(_sql_deleteEmployee, _connection, transaction);
            deleteEmployeeCommand.Parameters.AddWithValue("@employeeId", employeeId);

            int rowsAffected = deleteEmployeeCommand.ExecuteNonQuery();
            if (rowsAffected > 0)
            {
                transaction.Commit();
                Console.WriteLine("Employee deleted.");
            }
            else
            {
                transaction.Rollback();
                Console.WriteLine("Employee not found.");
            }
        }
        catch
        {
            transaction.Rollback();
            throw;
        }

        CloseConnection();
    }

    private const string _sql_createScheduleIfNotExists = "IF NOT EXISTS (SELECT 1 FROM dbo.Schedule WHERE ScheduleDate = @scheduleDate) INSERT INTO dbo.Schedule (ScheduleDate) VALUES (@scheduleDate)";
    private const string _sql_getScheduleId = "SELECT Id FROM dbo.Schedule WHERE ScheduleDate = @scheduleDate";
    private const string _sql_shiftEmployeeExists = "SELECT COUNT(*) FROM dbo.ShiftEmployee WHERE ScheduleId = @scheduleId AND ShiftNumber = @shiftNumber AND EmployeeId = @employeeId";
    private const string _sql_addShiftEmployee = "INSERT INTO dbo.ShiftEmployee (ScheduleId, ShiftNumber, EmployeeId) VALUES (@scheduleId, @shiftNumber, @employeeId)";
    public void AddEmployeeToShift(DateTime scheduleDate, byte shiftNumber, int employeeId)
    {
        if (shiftNumber != 1 && shiftNumber != 2)
        {
            Console.WriteLine("Invalid shift number. Choose shift 1 or 2.");
            return;
        }

        if (!EmployeeExists(employeeId))
        {
            Console.WriteLine("Employee not found. Shift assignment cancelled.");
            return;
        }

        OpenConnection();

        SqlCommand createScheduleCommand = new SqlCommand(_sql_createScheduleIfNotExists, _connection);
        createScheduleCommand.Parameters.Add("@scheduleDate", System.Data.SqlDbType.Date).Value = scheduleDate.Date;
        createScheduleCommand.ExecuteNonQuery();

        SqlCommand getScheduleIdCommand = new SqlCommand(_sql_getScheduleId, _connection);
        getScheduleIdCommand.Parameters.Add("@scheduleDate", System.Data.SqlDbType.Date).Value = scheduleDate.Date;
        int scheduleId = Convert.ToInt32(getScheduleIdCommand.ExecuteScalar());

        SqlCommand shiftEmployeeExistsCommand = new SqlCommand(_sql_shiftEmployeeExists, _connection);
        shiftEmployeeExistsCommand.Parameters.AddWithValue("@scheduleId", scheduleId);
        shiftEmployeeExistsCommand.Parameters.AddWithValue("@shiftNumber", shiftNumber);
        shiftEmployeeExistsCommand.Parameters.AddWithValue("@employeeId", employeeId);

        int assignmentCount = Convert.ToInt32(shiftEmployeeExistsCommand.ExecuteScalar());
        if (assignmentCount > 0)
        {
            Console.WriteLine("Employee is already assigned to this shift.");
            CloseConnection();
            return;
        }

        SqlCommand addShiftEmployeeCommand = new SqlCommand(_sql_addShiftEmployee, _connection);
        addShiftEmployeeCommand.Parameters.AddWithValue("@scheduleId", scheduleId);
        addShiftEmployeeCommand.Parameters.AddWithValue("@shiftNumber", shiftNumber);
        addShiftEmployeeCommand.Parameters.AddWithValue("@employeeId", employeeId);

        int rowsAffected = addShiftEmployeeCommand.ExecuteNonQuery();
        if (rowsAffected > 0)
        {
            Console.WriteLine("Employee added to shift.");
        }
        else
        {
            Console.WriteLine("Employee could not be added to shift.");
        }

        CloseConnection();
    }

    private const string _sql_getAllSchedules = "SELECT s.Id, s.ScheduleDate, st.ShiftNumber, st.StartTime, st.EndTime, e.Id, e.Name FROM dbo.Schedule s CROSS JOIN dbo.ShiftType st LEFT JOIN dbo.ShiftEmployee se ON s.Id = se.ScheduleId AND st.ShiftNumber = se.ShiftNumber LEFT JOIN dbo.Employee e ON se.EmployeeId = e.Id ORDER BY s.ScheduleDate, st.ShiftNumber, e.Name";
    public void GetAllSchedules()
    {
        OpenConnection();
        SqlCommand command = new SqlCommand(_sql_getAllSchedules, _connection);

        SqlDataReader reader = command.ExecuteReader();
        if (reader.HasRows)
        {
            DateTime? previousScheduleDate = null;
            byte? previousShiftNumber = null;

            while (reader.Read())
            {
                int scheduleId = reader.GetInt32(0);
                DateTime scheduleDate = reader.GetDateTime(1);
                byte shiftNumber = reader.GetByte(2);
                TimeSpan startTime = reader.GetTimeSpan(3);
                TimeSpan endTime = reader.GetTimeSpan(4);

                if (previousScheduleDate != scheduleDate.Date)
                {
                    Console.WriteLine();
                    Console.WriteLine($"Schedule ID: {scheduleId}, Date: {scheduleDate:yyyy-MM-dd}");
                    previousScheduleDate = scheduleDate.Date;
                    previousShiftNumber = null;
                }

                if (previousShiftNumber != shiftNumber)
                {
                    Console.WriteLine($"  Shift {shiftNumber}: {startTime:hh\\:mm}-{endTime:hh\\:mm}");
                    previousShiftNumber = shiftNumber;
                }

                if (!reader.IsDBNull(5))
                {
                    int employeeId = reader.GetInt32(5);
                    string employeeName = reader.GetString(6);
                    Console.WriteLine($"    Employee {employeeId}: {employeeName}");
                }
                else
                {
                    Console.WriteLine("    No employees assigned.");
                }
            }
        }
        else
        {
            Console.WriteLine("No schedule dates found.");
        }

        reader.Close();
        CloseConnection();
    }

    private const string _sql_deleteShiftEmployeesForSchedule = "DELETE FROM dbo.ShiftEmployee WHERE ScheduleId = (SELECT Id FROM dbo.Schedule WHERE ScheduleDate = @scheduleDate)";
    private const string _sql_deleteSchedule = "DELETE FROM dbo.Schedule WHERE ScheduleDate = @scheduleDate";
    public void DeleteSchedule(DateTime scheduleDate)
    {
        OpenConnection();
        SqlTransaction transaction = _connection.BeginTransaction();

        try
        {
            SqlCommand deleteShiftEmployeesCommand = new SqlCommand(_sql_deleteShiftEmployeesForSchedule, _connection, transaction);
            deleteShiftEmployeesCommand.Parameters.Add("@scheduleDate", System.Data.SqlDbType.Date).Value = scheduleDate.Date;
            deleteShiftEmployeesCommand.ExecuteNonQuery();

            SqlCommand deleteScheduleCommand = new SqlCommand(_sql_deleteSchedule, _connection, transaction);
            deleteScheduleCommand.Parameters.Add("@scheduleDate", System.Data.SqlDbType.Date).Value = scheduleDate.Date;

            int rowsAffected = deleteScheduleCommand.ExecuteNonQuery();
            if (rowsAffected > 0)
            {
                transaction.Commit();
                Console.WriteLine("Schedule date deleted.");
            }
            else
            {
                transaction.Rollback();
                Console.WriteLine("Schedule date not found.");
            }
        }
        catch
        {
            transaction.Rollback();
            throw;
        }

        CloseConnection();
    }

    private const string _sql_getScheduleIdForDate = "SELECT Id FROM dbo.Schedule WHERE ScheduleDate = @scheduleDate";
    private const string _sql_deleteEmployeeFromShift = "DELETE FROM dbo.ShiftEmployee WHERE ScheduleId = @scheduleId AND ShiftNumber = @shiftNumber AND EmployeeId = @employeeId";
    public void DeleteEmployeeFromShift(DateTime scheduleDate, byte shiftNumber, int employeeId)
    {
        if (shiftNumber != 1 && shiftNumber != 2)
        {
            Console.WriteLine("Invalid shift number. Choose shift 1 or 2.");
            return;
        }

        OpenConnection();
        SqlCommand getScheduleIdCommand = new SqlCommand(_sql_getScheduleIdForDate, _connection);
        getScheduleIdCommand.Parameters.Add("@scheduleDate", System.Data.SqlDbType.Date).Value = scheduleDate.Date;

        object scheduleIdResult = getScheduleIdCommand.ExecuteScalar();
        if (scheduleIdResult == null)
        {
            Console.WriteLine("Schedule date not found.");
            CloseConnection();
            return;
        }

        SqlCommand deleteCommand = new SqlCommand(_sql_deleteEmployeeFromShift, _connection);
        deleteCommand.Parameters.AddWithValue("@scheduleId", Convert.ToInt32(scheduleIdResult));
        deleteCommand.Parameters.AddWithValue("@shiftNumber", shiftNumber);
        deleteCommand.Parameters.AddWithValue("@employeeId", employeeId);

        int rowsAffected = deleteCommand.ExecuteNonQuery();
        if (rowsAffected > 0)
        {
            Console.WriteLine("Employee removed from shift.");
        }
        else
        {
            Console.WriteLine("Employee is not assigned to this shift.");
        }

        CloseConnection();
    }

    private const string _sql_getEmployeeShifts = "SELECT s.ScheduleDate, st.ShiftNumber, st.StartTime, st.EndTime FROM dbo.ShiftEmployee se INNER JOIN dbo.Schedule s ON se.ScheduleId = s.Id INNER JOIN dbo.ShiftType st ON se.ShiftNumber = st.ShiftNumber WHERE se.EmployeeId = @employeeId ORDER BY s.ScheduleDate, st.ShiftNumber";
    public void GetEmployeeShifts(int employeeId)
    {
        OpenConnection();
        SqlCommand command = new SqlCommand(_sql_getEmployeeShifts, _connection);
        command.Parameters.AddWithValue("@employeeId", employeeId);

        SqlDataReader reader = command.ExecuteReader();
        if (reader.HasRows)
        {
            Console.WriteLine($"Shifts for employee {employeeId}:");

            while (reader.Read())
            {
                DateTime scheduleDate = reader.GetDateTime(0);
                byte shiftNumber = reader.GetByte(1);
                TimeSpan startTime = reader.GetTimeSpan(2);
                TimeSpan endTime = reader.GetTimeSpan(3);

                Console.WriteLine($"{scheduleDate:yyyy-MM-dd}: Shift {shiftNumber}, {startTime:hh\\:mm}-{endTime:hh\\:mm}");
            }
        }
        else
        {
            Console.WriteLine("Employee has no assigned shifts.");
        }

        reader.Close();
        CloseConnection();
    }
}
