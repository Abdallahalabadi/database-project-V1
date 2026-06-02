using System.Data.SQLite;

namespace WarqERP.Models;

public class Database
{
    public static string ConnectionString = "Data Source=database/erp_system.db;Version=3;Pooling=False;BusyTimeout=10000;Journal Mode=Delete;";

    public static SQLiteConnection GetConnection()
    {
        SQLiteConnection con = new SQLiteConnection(ConnectionString);
        con.Open();

        using (SQLiteCommand cmd = con.CreateCommand())
        {
            cmd.CommandText = "PRAGMA foreign_keys = ON; PRAGMA busy_timeout = 10000; PRAGMA journal_mode = DELETE;";
            cmd.ExecuteNonQuery();
        }

        return con;
    }

    public static void CreateDatabase()
    {
        Directory.CreateDirectory("database");

        using SQLiteConnection con = GetConnection();

        using (SQLiteCommand cmd = con.CreateCommand())
        {
            cmd.CommandText = @"
            CREATE TABLE IF NOT EXISTS Category
            (
                CategoryId INTEGER PRIMARY KEY AUTOINCREMENT,
                CategoryName TEXT NOT NULL UNIQUE,
                Description TEXT
            );

            CREATE TABLE IF NOT EXISTS Supplier
            (
                SupplierId INTEGER PRIMARY KEY AUTOINCREMENT,
                SupplierName TEXT NOT NULL,
                ContactNumber TEXT NOT NULL,
                Email TEXT NOT NULL,
                Address TEXT NOT NULL
            );

            CREATE TABLE IF NOT EXISTS Employee
            (
                EmployeeId INTEGER PRIMARY KEY AUTOINCREMENT,
                FirstName TEXT NOT NULL,
                LastName TEXT NOT NULL,
                Position TEXT NOT NULL,
                Email TEXT NOT NULL,
                Phone TEXT NOT NULL,
                Salary REAL NOT NULL
            );

            CREATE TABLE IF NOT EXISTS Product
            (
                ProductId INTEGER PRIMARY KEY AUTOINCREMENT,
                ProductName TEXT NOT NULL,
                CategoryId INTEGER NOT NULL,
                Price REAL NOT NULL,
                StockQuantity INTEGER NOT NULL,
                FOREIGN KEY (CategoryId) REFERENCES Category(CategoryId)
            );

            CREATE TABLE IF NOT EXISTS PurchaseOrder
            (
                PurchaseOrderId INTEGER PRIMARY KEY AUTOINCREMENT,
                OrderDate TEXT NOT NULL,
                SupplierId INTEGER NOT NULL,
                EmployeeId INTEGER NOT NULL,
                TotalAmount REAL NOT NULL,
                Status TEXT NOT NULL,
                FOREIGN KEY (SupplierId) REFERENCES Supplier(SupplierId),
                FOREIGN KEY (EmployeeId) REFERENCES Employee(EmployeeId)
            );

            CREATE TABLE IF NOT EXISTS PurchaseOrderDetails
            (
                DetailId INTEGER PRIMARY KEY AUTOINCREMENT,
                PurchaseOrderId INTEGER NOT NULL,
                ProductId INTEGER NOT NULL,
                Quantity INTEGER NOT NULL,
                UnitPrice REAL NOT NULL,
                FOREIGN KEY (PurchaseOrderId) REFERENCES PurchaseOrder(PurchaseOrderId) ON DELETE CASCADE,
                FOREIGN KEY (ProductId) REFERENCES Product(ProductId)
            );
            ";
            cmd.ExecuteNonQuery();
        }

        using (SQLiteCommand checkCommand = con.CreateCommand())
        {
            checkCommand.CommandText = "SELECT COUNT(*) FROM Supplier";
            long count = (long)checkCommand.ExecuteScalar();

            if (count == 0)
            {
                using SQLiteCommand seedCommand = con.CreateCommand();
                seedCommand.CommandText = @"
                INSERT INTO Category (CategoryName, Description) VALUES
                ('Electronics', 'Electronic devices and accessories'),
                ('Office', 'Office supplies and paper products'),
                ('Furniture', 'Office furniture items'),
                ('Stationery', 'Writing and stationery supplies');

                INSERT INTO Supplier (SupplierName, ContactNumber, Email, Address) VALUES
                ('Tech Supplies Co.', '0791234567', 'tech@supplies.com', 'Amman, Jordan'),
                ('Office Hub', '0797654321', 'sales@officehub.com', 'Zarqa, Jordan'),
                ('Global Furnish', '0784567890', 'contact@globalfurnish.com', 'Irbid, Jordan');

                INSERT INTO Employee (FirstName, LastName, Position, Email, Phone, Salary) VALUES
                ('Anwar', 'Maslamani', 'Purchase Officer', 'anwar@erp.com', '0791111111', 900),
                ('Abdullah', 'Madaat', 'HR Officer', 'abdullah@erp.com', '0792222222', 850),
                ('Lina', 'Khaled', 'Inventory Manager', 'lina@erp.com', '0793333333', 950);

                INSERT INTO Product (ProductName, CategoryId, Price, StockQuantity) VALUES
                ('Laptop', 1, 700, 12),
                ('Printer Paper', 2, 8, 65),
                ('Office Chair', 3, 120, 9),
                ('Pen Box', 4, 4, 44),
                ('Keyboard', 1, 25, 18);

                INSERT INTO PurchaseOrder (OrderDate, SupplierId, EmployeeId, TotalAmount, Status) VALUES
                ('2026-01-15', 1, 1, 2800, 'Completed'),
                ('2026-02-08', 2, 3, 80, 'Pending'),
                ('2026-03-05', 3, 2, 240, 'Completed');

                INSERT INTO PurchaseOrderDetails (PurchaseOrderId, ProductId, Quantity, UnitPrice) VALUES
                (1, 1, 4, 700),
                (2, 2, 10, 8),
                (3, 3, 2, 120);
                ";
                seedCommand.ExecuteNonQuery();
            }
        }
    }
}
