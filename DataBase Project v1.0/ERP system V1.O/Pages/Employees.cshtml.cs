using WarqERP.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WarqERP.Pages;

public class EmployeesModel : PageModel
{
    public Employee employee { get; set; } = new Employee();
    public List<Employee> employees = new List<Employee>();
    public string statusMessage = "";

    public void OnGet(int? editId)
    {
        Employee employeeRecord = new Employee();

        if (editId.HasValue)
        {
            employee = employeeRecord.GetEmployeeById(editId.Value);
        }

        employees = employeeRecord.GetEmployees();
        statusMessage = TempData["statusMessage"] as string ?? "";
    }

    public IActionResult OnPostSave()
    {
        int employeeId = Convert.ToInt32(Request.Form["EmployeeId"].ToString() == "" ? "0" : Request.Form["EmployeeId"].ToString());
        string firstName = Request.Form["FirstName"];
        string lastName = Request.Form["LastName"];
        string position = Request.Form["Position"];
        string email = Request.Form["Email"];
        string phone = Request.Form["Phone"];
        decimal salary = Convert.ToDecimal(Request.Form["Salary"].ToString() == "" ? "0" : Request.Form["Salary"].ToString());

        Employee employeeRecord = new Employee();
        statusMessage = employeeRecord.SaveData(employeeId, firstName, lastName, position, email, phone, salary);

        TempData["statusMessage"] = statusMessage;
        return RedirectToPage();
    }

    public IActionResult OnPostDelete(int id)
    {
        Employee employeeRecord = new Employee();
        statusMessage = employeeRecord.DeleteData(id);

        TempData["statusMessage"] = statusMessage;
        return RedirectToPage();
    }
}
