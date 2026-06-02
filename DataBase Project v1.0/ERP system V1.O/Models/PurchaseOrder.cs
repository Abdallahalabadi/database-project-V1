using System.Data.SQLite;

namespace WarqERP.Models;

public class PurchaseOrder
{
    public int PurchaseOrderId { get; set; }
    public string OrderDate { get; set; } = "";
    public int SupplierId { get; set; }
    public string SupplierName { get; set; } = "";
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = "";
    public int ProductId { get; set; }
    public string ProductName { get; set; } = "";
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = "";

    public List<PurchaseOrder> GetPurchaseOrders()
    {
        List<PurchaseOrder> orderList = new List<PurchaseOrder>();

        using SQLiteConnection con = Database.GetConnection();
        using SQLiteCommand cmd = con.CreateCommand();
        cmd.CommandText = @"
        SELECT po.PurchaseOrderId, po.OrderDate, po.SupplierId, s.SupplierName,
               po.EmployeeId, e.FirstName || ' ' || e.LastName AS EmployeeName,
               pod.ProductId, p.ProductName, pod.Quantity, pod.UnitPrice,
               po.TotalAmount, po.Status
        FROM PurchaseOrder po
        INNER JOIN Supplier s ON po.SupplierId = s.SupplierId
        INNER JOIN Employee e ON po.EmployeeId = e.EmployeeId
        INNER JOIN PurchaseOrderDetails pod ON po.PurchaseOrderId = pod.PurchaseOrderId
        INNER JOIN Product p ON pod.ProductId = p.ProductId
        ORDER BY po.PurchaseOrderId DESC";

        using SQLiteDataReader dr = cmd.ExecuteReader();

        while (dr.Read())
        {
            PurchaseOrder order = new PurchaseOrder();
            order.PurchaseOrderId = Convert.ToInt32(dr["PurchaseOrderId"]);
            order.OrderDate = dr["OrderDate"].ToString() ?? "";
            order.SupplierId = Convert.ToInt32(dr["SupplierId"]);
            order.SupplierName = dr["SupplierName"].ToString() ?? "";
            order.EmployeeId = Convert.ToInt32(dr["EmployeeId"]);
            order.EmployeeName = dr["EmployeeName"].ToString() ?? "";
            order.ProductId = Convert.ToInt32(dr["ProductId"]);
            order.ProductName = dr["ProductName"].ToString() ?? "";
            order.Quantity = Convert.ToInt32(dr["Quantity"]);
            order.UnitPrice = Convert.ToDecimal(dr["UnitPrice"]);
            order.TotalAmount = Convert.ToDecimal(dr["TotalAmount"]);
            order.Status = dr["Status"].ToString() ?? "";

            orderList.Add(order);
        }

        return orderList;
    }

    public void SavePurchaseOrder(int supplierId, int employeeId, int productId, int quantity, string status)
    {
        using SQLiteConnection con = Database.GetConnection();
        using SQLiteTransaction transaction = con.BeginTransaction();

        decimal unitPrice;

        using (SQLiteCommand productCommand = con.CreateCommand())
        {
            productCommand.Transaction = transaction;
            productCommand.CommandText = "SELECT Price FROM Product WHERE ProductId = @ProductId";
            productCommand.Parameters.AddWithValue("@ProductId", productId);
            unitPrice = Convert.ToDecimal(productCommand.ExecuteScalar());
        }

        decimal totalAmount = unitPrice * quantity;
        long purchaseOrderId;

        using (SQLiteCommand orderCommand = con.CreateCommand())
        {
            orderCommand.Transaction = transaction;
            orderCommand.CommandText = @"
            INSERT INTO PurchaseOrder (OrderDate, SupplierId, EmployeeId, TotalAmount, Status)
            VALUES (@OrderDate, @SupplierId, @EmployeeId, @TotalAmount, @Status);
            SELECT last_insert_rowid();";

            orderCommand.Parameters.AddWithValue("@OrderDate", DateTime.Today.ToString("yyyy-MM-dd"));
            orderCommand.Parameters.AddWithValue("@SupplierId", supplierId);
            orderCommand.Parameters.AddWithValue("@EmployeeId", employeeId);
            orderCommand.Parameters.AddWithValue("@TotalAmount", totalAmount);
            orderCommand.Parameters.AddWithValue("@Status", status);

            purchaseOrderId = (long)orderCommand.ExecuteScalar();
        }

        using (SQLiteCommand detailCommand = con.CreateCommand())
        {
            detailCommand.Transaction = transaction;
            detailCommand.CommandText = @"
            INSERT INTO PurchaseOrderDetails (PurchaseOrderId, ProductId, Quantity, UnitPrice)
            VALUES (@PurchaseOrderId, @ProductId, @Quantity, @UnitPrice)";

            detailCommand.Parameters.AddWithValue("@PurchaseOrderId", purchaseOrderId);
            detailCommand.Parameters.AddWithValue("@ProductId", productId);
            detailCommand.Parameters.AddWithValue("@Quantity", quantity);
            detailCommand.Parameters.AddWithValue("@UnitPrice", unitPrice);

            detailCommand.ExecuteNonQuery();
        }

        if (status == "Completed")
        {
            using SQLiteCommand stockCommand = con.CreateCommand();
            stockCommand.Transaction = transaction;
            stockCommand.CommandText = "UPDATE Product SET StockQuantity = StockQuantity + @Quantity WHERE ProductId = @ProductId";
            stockCommand.Parameters.AddWithValue("@Quantity", quantity);
            stockCommand.Parameters.AddWithValue("@ProductId", productId);
            stockCommand.ExecuteNonQuery();
        }

        transaction.Commit();
    }

    public void DeletePurchaseOrder(int purchaseOrderId)
    {
        using SQLiteConnection con = Database.GetConnection();
        using SQLiteTransaction transaction = con.BeginTransaction();

        using (SQLiteCommand deleteDetails = con.CreateCommand())
        {
            deleteDetails.Transaction = transaction;
            deleteDetails.CommandText = "DELETE FROM PurchaseOrderDetails WHERE PurchaseOrderId = @PurchaseOrderId";
            deleteDetails.Parameters.AddWithValue("@PurchaseOrderId", purchaseOrderId);
            deleteDetails.ExecuteNonQuery();
        }

        using (SQLiteCommand deleteOrder = con.CreateCommand())
        {
            deleteOrder.Transaction = transaction;
            deleteOrder.CommandText = "DELETE FROM PurchaseOrder WHERE PurchaseOrderId = @PurchaseOrderId";
            deleteOrder.Parameters.AddWithValue("@PurchaseOrderId", purchaseOrderId);
            deleteOrder.ExecuteNonQuery();
        }

        transaction.Commit();
    }

public string SaveData(int supplierId, int employeeId, int productId, int quantity, string status)
{
    string errorMessage;

    try
    {
        SavePurchaseOrder(supplierId, employeeId, productId, quantity, status);
        errorMessage = "Saved successfully.";
    }
    catch
    {
        errorMessage = "Unable to save the changes. Please check the data and try again.";
    }

    return errorMessage;
}

public string DeleteData(int purchaseOrderId)
{
    string errorMessage;

    try
    {
        DeletePurchaseOrder(purchaseOrderId);
        errorMessage = "Deleted successfully.";
    }
    catch
    {
        errorMessage = "Unable to save the changes. Please check the data and try again.";
    }

    return errorMessage;
}
}
