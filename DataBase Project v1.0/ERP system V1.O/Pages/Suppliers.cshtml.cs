using WarqERP.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WarqERP.Pages;

public class SuppliersModel : PageModel
{
    public Supplier supplier { get; set; } = new Supplier();
    public List<Supplier> suppliers = new List<Supplier>();
    public string statusMessage = "";

    public void OnGet(int? editId)
    {
        Supplier supplierRecord = new Supplier();

        if (editId.HasValue)
        {
            supplier = supplierRecord.GetSupplierById(editId.Value);
        }

        suppliers = supplierRecord.GetSuppliers();
        statusMessage = TempData["statusMessage"] as string ?? "";
    }

    public IActionResult OnPostSave()
    {
        int supplierId = Convert.ToInt32(Request.Form["SupplierId"].ToString() == "" ? "0" : Request.Form["SupplierId"].ToString());
        string supplierName = Request.Form["SupplierName"];
        string contactNumber = Request.Form["ContactNumber"];
        string email = Request.Form["Email"];
        string address = Request.Form["Address"];

        Supplier supplierRecord = new Supplier();
        statusMessage = supplierRecord.SaveData(supplierId, supplierName, contactNumber, email, address);

        TempData["statusMessage"] = statusMessage;
        return RedirectToPage();
    }

    public IActionResult OnPostDelete(int id)
    {
        Supplier supplierRecord = new Supplier();
        statusMessage = supplierRecord.DeleteData(id);

        TempData["statusMessage"] = statusMessage;
        return RedirectToPage();
    }
}
