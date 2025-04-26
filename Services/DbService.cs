using System.ComponentModel;
using Microsoft.Data.Sqlite;

public static class DbService
{
        static string connectionString = $"Data Source={FilePaths.DatabasePath}";
        static string buyerPath = FilePaths.BuyersPath;
        static string productsPath = FilePaths.ProductsPath;

        public static async Task InsertPuchaseOrders(List<PurchaseOrder> purchaseOrders)
        {
                using var connection = new SqliteConnection(connectionString);
                await connection.OpenAsync();

                foreach(var purchaseOrder in purchaseOrders)
                {
                        using var command = connection.CreateCommand();

                        command.CommandText = $"Insert Into PurchaseOrderLines (BuyerId, OrderDate, ProductCode, Quantity) "
                        + $" values('{purchaseOrder.BuyerId}', "
                        + $"'{purchaseOrder.OrderDate}', "
                        + $"'{purchaseOrder.ProductCode}', "
                        + $"{purchaseOrder.Quantity})";

                        await command.ExecuteScalarAsync();
                        //Console.WriteLine(command.CommandText);
                }


                await connection.CloseAsync();
        }

        public static async Task<HashSet<string>> GetHashSet(string table)
        {
                HashSet<string> hashSet = new HashSet<string>();

                using var connection = new SqliteConnection(connectionString);
                await connection.OpenAsync(); 

                using var command = connection.CreateCommand();
                command.CommandText = $"Select * from {table}";
                
                using var reader = await command.ExecuteReaderAsync();

                while(reader.Read()){
                        string result = reader.GetString(0);
                        hashSet.Add(result);
                }

                await connection.CloseAsync();
                return hashSet;
        }

        public static async Task LoadInitialDataIntoDb()
        {
                var buyerRecords = await FileReader<Buyer>.ReadFile(buyerPath);
                var productRecords = await FileReader<Product>.ReadFile(productsPath);

                using var connection = new SqliteConnection(connectionString);
                await connection.OpenAsync();
                foreach (var buyer in buyerRecords.SuccessfullRecords)
                {
                        using var command = connection.CreateCommand();
                        command.CommandText = $"INSERT INTO Buyer (BuyerId) values('{buyer.BuyerId}')";
                        await command.ExecuteScalarAsync();
                }
                foreach (var product in productRecords.SuccessfullRecords)
                {
                        using var command = connection.CreateCommand();
                        command.CommandText = $"INSERT INTO Product (ProductCode) values('{product.ProductCode}')";
                        await command.ExecuteScalarAsync();
                }
                await connection.CloseAsync();
        }
}