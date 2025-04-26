public static class FilePaths
{

    public static string DatabasePath = Path.Combine(AppContext.BaseDirectory, "Data", "database.db");
    public static string ProductsPath = Path.Combine(AppContext.BaseDirectory, "Data", "products.csv");
    public static string BuyersPath = Path.Combine(AppContext.BaseDirectory, "Data", "buyers.csv");
    public static string PurchaseOrdersPath = Path.Combine(AppContext.BaseDirectory, "Data", "purchase_orders.csv");
}