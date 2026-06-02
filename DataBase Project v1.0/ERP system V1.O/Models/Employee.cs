using System.ComponentModel.DataAnnotations;
using System.Data.SQLite;

namespace WarqERP.Models;

public class Employee
{
    public int EmployeeId { get; set; }

    [Required]
    public string FirstName { get; set; } = "";

    [Required]
    public string LastName { get; set; } = "";

    [Required]
    public string Position { get; set; } = "";

    [Required]
    public string Email { get; set; } = "";

    [Required]
    public string Phone { get; set; } = "";

    public decimal Salary { get; set; }

    public List<Employee> GetEmployees()
    {
        List<Employee> employeeList = new List<Employee>();

        using SQLiteConnection con = Database.GetConnection();
        using SQLiteCommand cmd = con.CreateCommand();
        cmd.CommandText = "SELECT EmployeeId, FirstName, LastName, Position, Email, Phone, Salary FROM Employee ORDER BY EmployeeId DESC";

        using SQLiteDataReader dr = cmd.ExecuteReader();

        while (dr.Read())
        {
            Employee employee = new Employee();
            employee.EmployeeId = Convert.ToInt32(dr["EmployeeId"]);
            employee.FirstName = dr["FirstName"].ToString() ?? "";
            employee.LastName = dr["LastName"].ToString() ?? "";
            employee.Position = dr["Position"].ToString() ?? "";
            employee.Email = dr["Email"].ToString() ?? "";
            employee.Phone = dr["Phone"].ToString() ?? "";
            employee.Salary = Convert.ToDecimal(dr["Salary"]);

            employeeList.Add(employee);
        }

        return employeeList;
    }

    public Employee GetEmployeeById(int employeeId)
    {
        Employee employee = new Employee();

        using SQLiteConnection con = Database.GetConnection();
        using SQLiteCommand cmd = con.CreateCommand();
        cmd.CommandText = "SELECT EmployeeId, FirstName, LastName, Position, Email, Phone, Salary FROM Employee WHERE EmployeeId = @EmployeeId";
        cmd.Parameters.AddWithValue("@EmployeeId", employeeId);

        using SQLiteDataReader dr = cmd.ExecuteReader();

        if (dr.Read())
        {
            employee.EmployeeId = Convert.ToInt32(dr["EmployeeId"]);
            employee.FirstName = dr["FirstName"].ToString() ?? "";
            employee.LastName = dr["LastName"].ToString() ?? "";
            employee.Position = dr["Position"].ToString() ?? "";
            employee.Email = dr["Email"].ToString() ?? "";
            employee.Phone = dr["Phone"].ToString() ?? "";
            employee.Salary = Convert.ToDecimal(dr["Salary"]);
        }

        return employee;
    }

    public void SaveEmployee(Employee employee)
    {
        using SQLiteConnection con = Database.GetConnection();
        using SQLiteCommand cmd = con.CreateCommand();

        if (employee.EmployeeId == 0)
        {
            cmd.CommandText = "INSERT INTO Employee (FirstName, LastName, Position, Email, Phone, Salary) VALUES (@FirstName, @LastName, @Position, @Email, @Phone, @Salary)";
        }
        else
        {
            cmd.CommandText = "UPDATE Employee SET FirstName = @FirstName, LastName = @LastName, Position = @Position, Email = @Email, Phone = @Phone, Salary = @Salary WHERE EmployeeId = @EmployeeId";
            cmd.Parameters.AddWithValue("@EmployeeId", employee.EmployeeId);
        }

        cmd.Parameters.AddWithValue("@FirstName", employee.FirstName);
        cmd.Parameters.AddWithValue("@LastName", employee.LastName);
        cmd.Parameters.AddWithValue("@Position", employee.Position);
        cmd.Parameters.AddWithValue("@Email", employee.Email);
        cmd.Parameters.AddWithValue("@Phone", employee.Phone);
        cmd.Parameters.AddWithValue("@Salary", employee.Salary);

        cmd.ExecuteNonQuery();
    }

    public void DeleteEmployee(int employeeId)
    {
        using SQLiteConnection con = Database.GetConnection();
        using SQLiteTransaction transaction = con.BeginTransaction();

        using (SQLiteCommand deleteDetails = con.CreateCommand())
        {
            deleteDetails.Transaction = transaction;
            deleteDetails.CommandText = @"
                DELETE FROM PurchaseOrderDetails
                WHERE PurchaseOrderId IN
                (
                    SELECT PurchaseOrderId FROM PurchaseOrder WHERE EmployeeId = @EmployeeId
                )";
            deleteDetails.Parameters.AddWithValue("@EmployeeId", employeeId);
            deleteDetails.ExecuteNonQuery();
        }

        using (SQLiteCommand deleteOrders = con.CreateCommand())
        {
            deleteOrders.Transaction = transaction;
            deleteOrders.CommandText = "DELETE FROM PurchaseOrder WHERE EmployeeId = @EmployeeId";
            deleteOrders.Parameters.AddWithValue("@EmployeeId", employeeId);
            deleteOrders.ExecuteNonQuery();
        }

        using (SQLiteCommand deleteEmployee = con.CreateCommand())
        {
            deleteEmployee.Transaction = transaction;
            deleteEmployee.CommandText = "DELETE FROM Employee WHERE EmployeeId = @EmployeeId";
            deleteEmployee.Parameters.AddWithValue("@EmployeeId", employeeId);
            deleteEmployee.ExecuteNonQuery();
        }

        transaction.Commit();
    }

public string SaveData(int employeeId, string firstName, string lastName, string position, string email, string phone, decimal salary)
{
    string errorMessage;

    try
    {
        Employee employee = new Employee();
        employee.EmployeeId = employeeId;
        employee.FirstName = firstName;
        employee.LastName = lastName;
        employee.Position = position;
        employee.Email = email;
        employee.Phone = phone;
        employee.Salary = salary;

        SaveEmployee(employee);

        errorMessage = "Saved successfully.";
    }
    catch
    {
        errorMessage = "Unable to save the changes. Please check the data and try again.";
    }

    return errorMessage;
}

public string DeleteData(int employeeId)
{
    string errorMessage;

    try
    {
        DeleteEmployee(employeeId);
        errorMessage = "Deleted successfully.";
    }
    catch
    {
        errorMessage = "Unable to save the changes. Please check the data and try again.";
    }

    return errorMessage;
}
}
