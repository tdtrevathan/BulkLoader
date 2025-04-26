public static class PurchaseOrderProcessor
{
    public static async Task<PurchaseOrderProcessResult> ProcessPurchaseOrders()
    {
        var path = FilePaths.PurchaseOrdersPath;

        var records = await FileReader<PurchaseOrder>.ReadFile(path);

        var invalidRecords = new List<PurchaseOrder>();
        var validRecords = new List<PurchaseOrder>();

        foreach (var record in records.SuccessfullRecords)
        {
            var validation = await ValidationService.IsRecordValid(record);

            if (validation.IsBuyerIdValid
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

        return new PurchaseOrderProcessResult(){
            ValidPurchaseOrders = validRecords,
            InValidPurchaseOrders = invalidRecords,
            UnprocessablePurchaseOrders = records.UnprocessableRecords
        };
    }
}