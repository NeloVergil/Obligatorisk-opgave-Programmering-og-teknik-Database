using PedrosCantinaShiftScheduleClasslib;

PedrosCantinaShiftScheduleRepository repository = new PedrosCantinaShiftScheduleRepository();
Console.WriteLine("Enter Employee ID to get contact info:");
int employeeId = Convert.ToInt32(Console.ReadLine());
repository.GetEmployeeContactInfo(employeeId);