using WarqERP.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WarqERP.Pages;

public class InventoryModel : PageModel
{
    public List<Product> products = new List<Product>();

    public void OnGet()
    {
        Product product = new Product();
        products = product.GetProducts();
    }
}
