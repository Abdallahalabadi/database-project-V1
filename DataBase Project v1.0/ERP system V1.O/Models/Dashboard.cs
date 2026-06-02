using System.Data.SQLite;

namespace WarqERP.Models;

public class Dashboard
{
    public int TotalSuppliers { get; set; }
    public int TotalInventory { get; set; }
    public int TotalOrders { get; set; }
    public int LowStockCount { get; set; }

    public Dashboard GetDashboardData()
    {
        Dashboard dashboard = new Dashboard();

        using SQLiteConnection con = Database.GetConnection();

        dashboard.TotalSuppliers = Convert.ToInt32(ExecuteScalar(con, "SELECT COUNT(*) FROM Supplier"));
        dashboard.TotalInventory = Convert.ToInt32(ExecuteScalar(con, "SELECT IFNULL(SUM(StockQuantity), 0) FROM Product"));
        dashboard.TotalOrders = Convert.ToInt32(ExecuteScalar(con, "SELECT COUNT(*) FROM PurchaseOrder"));
        dashboard.LowStockCount = Convert.ToInt32(ExecuteScalar(con, "SELECT COUNT(*) FROM Product WHERE StockQuantity < 20"));

        return dashboard;
    }

    private object ExecuteScalar(SQLiteConnection con, string query)
    {
        using SQLiteCommand cmd = con.CreateCommand();
        cmd.CommandText = query;
        return cmd.ExecuteScalar() ?? 0;
    }
}
