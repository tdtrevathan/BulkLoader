using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Runtime.CompilerServices;
using CsvHelper;

var path = "/Users/Tim/reposVSCode/BulkLoader/csv_files/purchase_orders.csv";

var buyers = await DbService.GetHashSet("Buyer");
var products = await DbService.GetHashSet("Product");

var records = await FileReader<PurchaseOrder>.ReadFile(path);

var invalidRecords = new List<PurchaseOrder>();
var validRecords = new List<PurchaseOrder>();

foreach(var record in records.SuccessfullRecords){
    var validation = IsRecordValid(record);

    if(validation.IsBuyerIdValid
        && validation.IsProductCodeValid
        && validation.IsQuantityValid)
    {
        validRecords.Add(record);
    }
    else
    {
        invalidRecords.Add(record);
    }
}

foreach(var record in validRecords){
    Console.WriteLine("Valid Record");    
    Console.WriteLine($"BuyerId: {record.BuyerId}");    
    Console.WriteLine($"ProductCode: {record.ProductCode}");    
    Console.WriteLine($"OrderDate: {record.OrderDate}");    
    Console.WriteLine($"Quantity: {record.Quantity}");    
    Console.WriteLine();
}

foreach(var record in invalidRecords){
    Console.WriteLine($"Invalid Record:");
    Console.WriteLine($"BuyerId: {record.BuyerId}");    
    Console.WriteLine($"ProductCode: {record.ProductCode}");    
    Console.WriteLine($"OrderDate: {record.OrderDate}");    
    Console.WriteLine($"Quantity: {record.Quantity}");    
    Console.WriteLine();
}

foreach(var record in records.UnprocessableRecords){
    Console.WriteLine($"Unprocessable Record: {record}");
    Console.WriteLine();    
}

PurchaseOrderValidationResult IsRecordValid(PurchaseOrder purchaseOrder){
    return new PurchaseOrderValidationResult()
    {
        IsQuantityValid = purchaseOrder.Quantity >= 0,
        IsBuyerIdValid = buyers.Contains(purchaseOrder.BuyerId),
        IsProductCodeValid = products.Contains(purchaseOrder.ProductCode)
    };
}