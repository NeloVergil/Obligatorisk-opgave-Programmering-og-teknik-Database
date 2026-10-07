using PedrosCantinaShiftScheduleClasslib;

PedrosCantinaShiftScheduleRepository repository = new PedrosCantinaShiftScheduleRepository();

bool running = true;

while (running)
{
    Console.WriteLine();
    Console.WriteLine("Employee menu");
    Console.WriteLine("1. Create employee");
    Console.WriteLine("2. Get employee contact info");
    Console.WriteLine("3. Get all employees");
    Console.WriteLine("4. Update employee");
    Console.WriteLine("5. Delete employee");
    Console.WriteLine("6. Exit");
    Console.WriteLine("What do you want to do?");

    string choice = Console.ReadLine();

    switch (choice)
    {
        case "1":
            Console.WriteLine("Enter employee name:");
            string name = Console.ReadLine();

            Console.WriteLine("Enter employee phone:");
            string phone = Console.ReadLine();

            Console.WriteLine("Enter employee email:");
            string email = Console.ReadLine();

            repository.CreateEmployee(name, phone, email);
            break;

        case "2":
            Console.WriteLine("Enter Employee ID to get contact info:");
            int contactInfoEmployeeId = Convert.ToInt32(Console.ReadLine());
            repository.GetEmployeeContactInfo(contactInfoEmployeeId);
            break;

        case "3":
            repository.GetAllEmployees();
            break;

        case "4":
            Console.WriteLine("Enter employee ID to update:");
            repository.GetAllEmployees();
            int updateEmployeeId = Convert.ToInt32(Console.ReadLine());

            if (repository.EmployeeExists(updateEmployeeId))
            {
                Console.WriteLine("Enter new employee name:");
                string newName = Console.ReadLine();

                Console.WriteLine("Enter new employee phone:");
                string newPhone = Console.ReadLine();

                Console.WriteLine("Enter new employee email:");
                string newEmail = Console.ReadLine();

                repository.UpdateEmployee(updateEmployeeId, newName, newPhone, newEmail);
            }
            else
            {
                Console.WriteLine("Employee not found. Update cancelled.");
            }
            break;

        case "5":
            Console.WriteLine("Enter employee ID to delete:");
            repository.GetAllEmployees();
            int deleteEmployeeId = Convert.ToInt32(Console.ReadLine());
            repository.DeleteEmployee(deleteEmployeeId);
            break;

        case "6":
            running = false;
            Console.WriteLine("Goodbye.");
            break;

        default:
            Console.WriteLine("Invalid choice.");
            break;
    }
}