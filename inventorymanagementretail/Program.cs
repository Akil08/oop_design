using InventoryManagementRetail;

var store = new InventoryStore();
store.AddOrUpdateProduct(new Product("Milk", "Dairy", 45m, 20, 5));
store.AddOrUpdateProduct(new Product("Bread", "Bakery", 30m, 12, 4));

var alert = store.Sell("Milk", 17);
Console.WriteLine($"Milk stock: {store.GetProduct("Milk").StockQuantity}");
if (alert is not null)
{
    Console.WriteLine(alert);
}

store.ReceiveStock(new PurchaseOrder("Milk", 25));
store.Discontinue("Bread");

Console.WriteLine($"Milk stock after delivery: {store.GetProduct("Milk").StockQuantity}");
Console.WriteLine($"Bread discontinued: {store.GetProduct("Bread").IsDiscontinued}");