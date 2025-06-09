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

                if (purchaseOrders.Count > 5000)
                {
                        List<Task> insertTasks = new List<Task>();
                        int batchSize = 5000;
                        for (int i = 0; i < purchaseOrders.Count; i += batchSize)
                        {
                                List<PurchaseOrder> batch = purchaseOrders.GetRange(i, Math.Min(batchSize, purchaseOrders.Count - i));
                                insertTasks.Add(InsertBatchAsync(connection, batch));
                        }
                        await Task.WhenAll(insertTasks);
                }
                else
                {
                        await InsertBatchAsync(connection, purchaseOrders);
                }

                await connection.CloseAsync();
        }

        private static async Task InsertBatchAsync(SqliteConnection connection, List<PurchaseOrder> batch)
        {
                if (batch == null || !batch.Any())
                {
                        return;
                }

                using var transaction = connection.BeginTransaction();
                try
                {
                        var command = connection.CreateCommand();
                        command.Transaction = transaction; // Associate command with the transaction

                        System.Text.StringBuilder sb = new System.Text.StringBuilder();
                        sb.Append("Insert Into PurchaseOrderLines (BuyerId, OrderDate, ProductCode, Quantity) VALUES ");

                        for (int i = 0; i < batch.Count; i++)
                        {
                                var po = batch[i];
                                sb.Append($"('{po.BuyerId}', '{po.OrderDate}', '{po.ProductCode}', {po.Quantity})");
                                if (i < batch.Count - 1)
                                {
                                        sb.Append(", ");
                                }
                        }
                        command.CommandText = sb.ToString();
                        await command.ExecuteNonQueryAsync(); // Use ExecuteNonQueryAsync for INSERT operations
                        await transaction.CommitAsync();
                }
                catch
                {
                        await transaction.RollbackAsync();
                        throw; // Re-throw the exception to be caught by the caller
                }
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