public static class ValidationService{

    public async static Task<PurchaseOrderValidationResult> IsRecordValid(PurchaseOrder purchaseOrder){
    
        var buyers = await DbService.GetHashSet("Buyer");
        var products = await DbService.GetHashSet("Product");

        return new PurchaseOrderValidationResult()
        {
            IsQuantityValid = purchaseOrder.Quantity >= 0,
            IsBuyerIdValid = buyers.Contains(purchaseOrder.BuyerId),
            IsProductCodeValid = products.Contains(purchaseOrder.ProductCode)
        };
    }
}