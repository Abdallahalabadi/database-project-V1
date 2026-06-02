using System.ComponentModel.DataAnnotations;
using System.Data.SQLite;

namespace WarqERP.Models;

public class Supplier
{
    public int SupplierId { get; set; }

    [Required]
    public string SupplierName { get; set; } = "";

    [Required]
    public string ContactNumber { get; set; } = "";

    [Required]
    public string Email { get; set; } = "";

    [Required]
    public string Address { get; set; } = "";

    public List<Supplier> GetSuppliers()
    {
        List<Supplier> supplierList = new List<Supplier>();

        using SQLiteConnection con = Database.GetConnection();
        using SQLiteCommand cmd = con.CreateCommand();
        cmd.CommandText = "SELECT SupplierId, SupplierName, ContactNumber, Email, Address FROM Supplier ORDER BY SupplierId DESC";

        using SQLiteDataReader dr = cmd.ExecuteReader();

        while (dr.Read())
        {
            Supplier supplier = new Supplier();
            supplier.SupplierId = Convert.ToInt32(dr["SupplierId"]);
            supplier.SupplierName = dr["SupplierName"].ToString() ?? "";
            supplier.ContactNumber = dr["ContactNumber"].ToString() ?? "";
            supplier.Email = dr["Email"].ToString() ?? "";
            supplier.Address = dr["Address"].ToString() ?? "";

            supplierList.Add(supplier);
        }

        return supplierList;
    }

    public Supplier GetSupplierById(int supplierId)
    {
        Supplier supplier = new Supplier();

        using SQLiteConnection con = Database.GetConnection();
        using SQLiteCommand cmd = con.CreateCommand();
        cmd.CommandText = "SELECT SupplierId, SupplierName, ContactNumber, Email, Address FROM Supplier WHERE SupplierId = @SupplierId";
        cmd.Parameters.AddWithValue("@SupplierId", supplierId);

        using SQLiteDataReader dr = cmd.ExecuteReader();

        if (dr.Read())
        {
            supplier.SupplierId = Convert.ToInt32(dr["SupplierId"]);
            supplier.SupplierName = dr["SupplierName"].ToString() ?? "";
            supplier.ContactNumber = dr["ContactNumber"].ToString() ?? "";
            supplier.Email = dr["Email"].ToString() ?? "";
            supplier.Address = dr["Address"].ToString() ?? "";
        }

        return supplier;
    }

    public void SaveSupplier(Supplier supplier)
    {
        using SQLiteConnection con = Database.GetConnection();
        using SQLiteCommand cmd = con.CreateCommand();

        if (supplier.SupplierId == 0)
        {
            cmd.CommandText = "INSERT INTO Supplier (SupplierName, ContactNumber, Email, Address) VALUES (@SupplierName, @ContactNumber, @Email, @Address)";
        }
        else
        {
            cmd.CommandText = "UPDATE Supplier SET SupplierName = @SupplierName, ContactNumber = @ContactNumber, Email = @Email, Address = @Address WHERE SupplierId = @SupplierId";
            cmd.Parameters.AddWithValue("@SupplierId", supplier.SupplierId);
        }

        cmd.Parameters.AddWithValue("@SupplierName", supplier.SupplierName);
        cmd.Parameters.AddWithValue("@ContactNumber", supplier.ContactNumber);
        cmd.Parameters.AddWithValue("@Email", supplier.Email);
        cmd.Parameters.AddWithValue("@Address", supplier.Address);

        cmd.ExecuteNonQuery();
    }

    public void DeleteSupplier(int supplierId)
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
                    SELECT PurchaseOrderId FROM PurchaseOrder WHERE SupplierId = @SupplierId
                )";
            deleteDetails.Parameters.AddWithValue("@SupplierId", supplierId);
            deleteDetails.ExecuteNonQuery();
        }

        using (SQLiteCommand deleteOrders = con.CreateCommand())
        {
            deleteOrders.Transaction = transaction;
            deleteOrders.CommandText = "DELETE FROM PurchaseOrder WHERE SupplierId = @SupplierId";
            deleteOrders.Parameters.AddWithValue("@SupplierId", supplierId);
            deleteOrders.ExecuteNonQuery();
        }

        using (SQLiteCommand deleteSupplier = con.CreateCommand())
        {
            deleteSupplier.Transaction = transaction;
            deleteSupplier.CommandText = "DELETE FROM Supplier WHERE SupplierId = @SupplierId";
            deleteSupplier.Parameters.AddWithValue("@SupplierId", supplierId);
            deleteSupplier.ExecuteNonQuery();
        }

        transaction.Commit();
    }

public string SaveData(int supplierId, string supplierName, string contactNumber, string email, string address)
{
    string errorMessage;

    try
    {
        Supplier supplier = new Supplier();
        supplier.SupplierId = supplierId;
        supplier.SupplierName = supplierName;
        supplier.ContactNumber = contactNumber;
        supplier.Email = email;
        supplier.Address = address;

        SaveSupplier(supplier);

        errorMessage = "Saved successfully.";
    }
    catch
    {
        errorMessage = "Unable to save the changes. Please check the data and try again.";
    }

    return errorMessage;
}

public string DeleteData(int supplierId)
{
    string errorMessage;

    try
    {
        DeleteSupplier(supplierId);
        errorMessage = "Deleted successfully.";
    }
    catch
    {
        errorMessage = "Unable to save the changes. Please check the data and try again.";
    }

    return errorMessage;
}
}
