using WarqERP.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WarqERP.Pages;

public class PurchaseOrdersModel : PageModel
{
    public List<Supplier> suppliers = new List<Supplier>();
    public List<Employee> employees = new List<Employee>();
    public List<Product> products = new List<Product>();
    public List<PurchaseOrder> orders = new List<PurchaseOrder>();
    public string statusMessage = "";

    public void OnGet()
    {
        LoadLists();
        statusMessage = TempData["statusMessage"] as string ?? "";
    }

    public IActionResult OnPostSave()
    {
        int supplierId = Convert.ToInt32(Request.Form["SupplierId"]);
        int employeeId = Convert.ToInt32(Request.Form["EmployeeId"]);
        int productId = Convert.ToInt32(Request.Form["ProductId"]);
        int quantity = Convert.ToInt32(Request.Form["Quantity"].ToString() == "" ? "1" : Request.Form["Quantity"].ToString());
        string status = Request.Form["Status"];

        PurchaseOrder purchaseOrder = new PurchaseOrder();
        statusMessage = purchaseOrder.SaveData(supplierId, employeeId, productId, quantity, status);

        TempData["statusMessage"] = statusMessage;
        return RedirectToPage();
    }

    public IActionResult OnPostDelete(int id)
    {
        PurchaseOrder purchaseOrder = new PurchaseOrder();
        statusMessage = purchaseOrder.DeleteData(id);

        TempData["statusMessage"] = statusMessage;
        return RedirectToPage();
    }

    private void LoadLists()
    {
        Supplier supplier = new Supplier();
        suppliers = supplier.GetSuppliers();

        Employee employee = new Employee();
        employees = employee.GetEmployees();

        Product product = new Product();
        products = product.GetProducts();

        PurchaseOrder purchaseOrder = new PurchaseOrder();
        orders = purchaseOrder.GetPurchaseOrders();
    }
}
