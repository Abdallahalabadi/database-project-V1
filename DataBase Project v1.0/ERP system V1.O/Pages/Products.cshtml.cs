using WarqERP.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WarqERP.Pages;

public class ProductsModel : PageModel
{
    public Product product { get; set; } = new Product();
    public List<Product> products = new List<Product>();
    public List<string> categories = new List<string>();
    public string statusMessage = "";

    public void OnGet(int? editId)
    {
        Product productRecord = new Product();
        categories = productRecord.GetCategories();

        if (editId.HasValue)
        {
            product = productRecord.GetProductById(editId.Value);
        }
        else if (categories.Count > 0)
        {
            product.CategoryName = categories[0];
        }

        products = productRecord.GetProducts();
        statusMessage = TempData["statusMessage"] as string ?? "";
    }

    public IActionResult OnPostSave()
    {
        int productId = Convert.ToInt32(Request.Form["ProductId"].ToString() == "" ? "0" : Request.Form["ProductId"].ToString());
        string productName = Request.Form["ProductName"];
        string categoryName = Request.Form["CategoryName"];
        decimal price = Convert.ToDecimal(Request.Form["Price"].ToString() == "" ? "0" : Request.Form["Price"].ToString());
        int stockQuantity = Convert.ToInt32(Request.Form["StockQuantity"].ToString() == "" ? "0" : Request.Form["StockQuantity"].ToString());

        Product productRecord = new Product();
        statusMessage = productRecord.SaveData(productId, productName, categoryName, price, stockQuantity);

        TempData["statusMessage"] = statusMessage;
        return RedirectToPage();
    }

    public IActionResult OnPostDelete(int id)
    {
        Product productRecord = new Product();
        statusMessage = productRecord.DeleteData(id);

        TempData["statusMessage"] = statusMessage;
        return RedirectToPage();
    }
}
