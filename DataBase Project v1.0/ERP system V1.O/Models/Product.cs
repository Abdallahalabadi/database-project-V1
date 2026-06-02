using System.ComponentModel.DataAnnotations;
using System.Data.SQLite;

namespace WarqERP.Models;

public class Product
{
    public int ProductId { get; set; }

    [Required]
    public string ProductName { get; set; } = "";

    [Required]
    public string CategoryName { get; set; } = "";

    public decimal Price { get; set; }
    public int StockQuantity { get; set; }

    public List<string> GetCategories()
    {
        List<string> categoryList = new List<string>();

        using SQLiteConnection con = Database.GetConnection();
        using SQLiteCommand cmd = con.CreateCommand();
        cmd.CommandText = "SELECT CategoryName FROM Category ORDER BY CategoryName";

        using SQLiteDataReader dr = cmd.ExecuteReader();

        while (dr.Read())
        {
            categoryList.Add(dr["CategoryName"].ToString() ?? "");
        }

        return categoryList;
    }

    public List<Product> GetProducts()
    {
        List<Product> productList = new List<Product>();

        using SQLiteConnection con = Database.GetConnection();
        using SQLiteCommand cmd = con.CreateCommand();
        cmd.CommandText = @"
        SELECT p.ProductId, p.ProductName, c.CategoryName, p.Price, p.StockQuantity
        FROM Product p
        INNER JOIN Category c ON p.CategoryId = c.CategoryId
        ORDER BY p.ProductId DESC";

        using SQLiteDataReader dr = cmd.ExecuteReader();

        while (dr.Read())
        {
            Product product = new Product();
            product.ProductId = Convert.ToInt32(dr["ProductId"]);
            product.ProductName = dr["ProductName"].ToString() ?? "";
            product.CategoryName = dr["CategoryName"].ToString() ?? "";
            product.Price = Convert.ToDecimal(dr["Price"]);
            product.StockQuantity = Convert.ToInt32(dr["StockQuantity"]);

            productList.Add(product);
        }

        return productList;
    }

    public Product GetProductById(int productId)
    {
        Product product = new Product();

        using SQLiteConnection con = Database.GetConnection();
        using SQLiteCommand cmd = con.CreateCommand();
        cmd.CommandText = @"
        SELECT p.ProductId, p.ProductName, c.CategoryName, p.Price, p.StockQuantity
        FROM Product p
        INNER JOIN Category c ON p.CategoryId = c.CategoryId
        WHERE p.ProductId = @ProductId";
        cmd.Parameters.AddWithValue("@ProductId", productId);

        using SQLiteDataReader dr = cmd.ExecuteReader();

        if (dr.Read())
        {
            product.ProductId = Convert.ToInt32(dr["ProductId"]);
            product.ProductName = dr["ProductName"].ToString() ?? "";
            product.CategoryName = dr["CategoryName"].ToString() ?? "";
            product.Price = Convert.ToDecimal(dr["Price"]);
            product.StockQuantity = Convert.ToInt32(dr["StockQuantity"]);
        }

        return product;
    }

    public void SaveProduct(Product product)
    {
        using SQLiteConnection con = Database.GetConnection();
        using SQLiteTransaction transaction = con.BeginTransaction();

        int categoryId = GetOrCreateCategoryId(con, transaction, product.CategoryName);

        using (SQLiteCommand cmd = con.CreateCommand())
        {
            cmd.Transaction = transaction;

            if (product.ProductId == 0)
            {
                cmd.CommandText = "INSERT INTO Product (ProductName, CategoryId, Price, StockQuantity) VALUES (@ProductName, @CategoryId, @Price, @StockQuantity)";
            }
            else
            {
                cmd.CommandText = "UPDATE Product SET ProductName = @ProductName, CategoryId = @CategoryId, Price = @Price, StockQuantity = @StockQuantity WHERE ProductId = @ProductId";
                cmd.Parameters.AddWithValue("@ProductId", product.ProductId);
            }

            cmd.Parameters.AddWithValue("@ProductName", product.ProductName);
            cmd.Parameters.AddWithValue("@CategoryId", categoryId);
            cmd.Parameters.AddWithValue("@Price", product.Price);
            cmd.Parameters.AddWithValue("@StockQuantity", product.StockQuantity);

            cmd.ExecuteNonQuery();
        }

        transaction.Commit();
    }

    private int GetOrCreateCategoryId(SQLiteConnection con, SQLiteTransaction transaction, string categoryName)
    {
        using SQLiteCommand findCommand = con.CreateCommand();
        findCommand.Transaction = transaction;
        findCommand.CommandText = "SELECT CategoryId FROM Category WHERE CategoryName = @CategoryName";
        findCommand.Parameters.AddWithValue("@CategoryName", categoryName);

        object? result = findCommand.ExecuteScalar();

        if (result != null)
        {
            return Convert.ToInt32(result);
        }

        using SQLiteCommand insertCommand = con.CreateCommand();
        insertCommand.Transaction = transaction;
        insertCommand.CommandText = "INSERT INTO Category (CategoryName, Description) VALUES (@CategoryName, 'Added from website'); SELECT last_insert_rowid();";
        insertCommand.Parameters.AddWithValue("@CategoryName", categoryName);

        return Convert.ToInt32(insertCommand.ExecuteScalar());
    }

    public void DeleteProduct(int productId)
    {
        using SQLiteConnection con = Database.GetConnection();
        using SQLiteTransaction transaction = con.BeginTransaction();

        using (SQLiteCommand deleteDetails = con.CreateCommand())
        {
            deleteDetails.Transaction = transaction;
            deleteDetails.CommandText = "DELETE FROM PurchaseOrderDetails WHERE ProductId = @ProductId";
            deleteDetails.Parameters.AddWithValue("@ProductId", productId);
            deleteDetails.ExecuteNonQuery();
        }

        using (SQLiteCommand deleteOrdersWithoutDetails = con.CreateCommand())
        {
            deleteOrdersWithoutDetails.Transaction = transaction;
            deleteOrdersWithoutDetails.CommandText = @"
                DELETE FROM PurchaseOrder
                WHERE PurchaseOrderId NOT IN
                (
                    SELECT PurchaseOrderId FROM PurchaseOrderDetails
                )";
            deleteOrdersWithoutDetails.ExecuteNonQuery();
        }

        using (SQLiteCommand deleteProduct = con.CreateCommand())
        {
            deleteProduct.Transaction = transaction;
            deleteProduct.CommandText = "DELETE FROM Product WHERE ProductId = @ProductId";
            deleteProduct.Parameters.AddWithValue("@ProductId", productId);
            deleteProduct.ExecuteNonQuery();
        }

        transaction.Commit();
    }

public string SaveData(int productId, string productName, string categoryName, decimal price, int stockQuantity)
{
    string errorMessage;

    try
    {
        Product product = new Product();
        product.ProductId = productId;
        product.ProductName = productName;
        product.CategoryName = categoryName;
        product.Price = price;
        product.StockQuantity = stockQuantity;

        SaveProduct(product);

        errorMessage = "Saved successfully.";
    }
    catch
    {
        errorMessage = "Unable to save the changes. Please check the data and try again.";
    }

    return errorMessage;
}

public string DeleteData(int productId)
{
    string errorMessage;

    try
    {
        DeleteProduct(productId);
        errorMessage = "Deleted successfully.";
    }
    catch
    {
        errorMessage = "Unable to save the changes. Please check the data and try again.";
    }

    return errorMessage;
}
}
