public class PurchaseOrderProcessResult{
    public List<PurchaseOrder> ValidPurchaseOrders {get; set;} = new List<PurchaseOrder>();
    public List<PurchaseOrder> InValidPurchaseOrders {get; set;} = new List<PurchaseOrder>();
    public List<string> UnprocessablePurchaseOrders {get; set;} = new List<string>();
}