public static class PurchaseOrderProcessor
{
    public static async Task<PurchaseOrderProcessResult> ProcessPurchaseOrders()
    {
        var path = FilePaths.PurchaseOrdersPath;

        var records = await FileReader<PurchaseOrder>.ReadFile(path);

        var invalidRecords = new List<PurchaseOrder>();
        var validRecords = new List<PurchaseOrder>();
        var processedProductCodes = new HashSet<string>();

        foreach (var record in records.SuccessfullRecords)
        {
            if (processedProductCodes.Contains(record.ProductCode))
            {
                invalidRecords.Add(record); // Duplicate product code
            }
            else
            {
                var validation = await ValidationService.IsRecordValid(record);

                if (validation.IsBuyerIdValid
                    && validation.IsProductCodeValid
                    && validation.IsQuantityValid)
                {
                    validRecords.Add(record);
                    processedProductCodes.Add(record.ProductCode); // Add to set after successful validation
                }
                else
                {
                    invalidRecords.Add(record);
                }
            }
        }

        return new PurchaseOrderProcessResult(){
            ValidPurchaseOrders = validRecords,
            InValidPurchaseOrders = invalidRecords,
            UnprocessablePurchaseOrders = records.UnprocessableRecords
        };
    }
}