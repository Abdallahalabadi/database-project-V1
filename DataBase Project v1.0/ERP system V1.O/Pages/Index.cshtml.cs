using WarqERP.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WarqERP.Pages;

public class IndexModel : PageModel
{
    public Dashboard dashboard = new Dashboard();
    public List<Product> products = new List<Product>();
    public List<PurchaseOrder> orders = new List<PurchaseOrder>();
    public int maxStock = 0;

    public void OnGet()
    {
        dashboard = dashboard.GetDashboardData();

        Product product = new Product();
        products = product.GetProducts().OrderBy(p => p.ProductId).ToList();

        PurchaseOrder purchaseOrder = new PurchaseOrder();
        orders = purchaseOrder.GetPurchaseOrders();

        if (products.Count > 0)
        {
            maxStock = products.Max(p => p.StockQuantity);
        }
    }
}
