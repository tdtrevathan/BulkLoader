public static class PurchaseOrderReultPrinter
{
    public static void PrintValidResults(List<PurchaseOrder> validRecords)
    {
        foreach (var record in validRecords)
        {
            Console.WriteLine("Valid Record");
            Console.WriteLine($"BuyerId: {record.BuyerId}");
            Console.WriteLine($"ProductCode: {record.ProductCode}");
            Console.WriteLine($"OrderDate: {record.OrderDate}");
            Console.WriteLine($"Quantity: {record.Quantity}");
            Console.WriteLine();
        }
    }

        public static void PrintInValidResults(List<PurchaseOrder> invalidRecords)
    {
        foreach (var record in invalidRecords)
        {
            Console.WriteLine($"Invalid Record:");
            Console.WriteLine($"BuyerId: {record.BuyerId}");
            Console.WriteLine($"ProductCode: {record.ProductCode}");
            Console.WriteLine($"OrderDate: {record.OrderDate}");
            Console.WriteLine($"Quantity: {record.Quantity}");
            Console.WriteLine();
        }
    }

        public static void PrintUnprocessableResults(List<string> unprocessableRecords)
    {
        foreach (var record in unprocessableRecords)
        {
            Console.WriteLine($"Unprocessable Record: {record}");
            Console.WriteLine();
        }
    }
}