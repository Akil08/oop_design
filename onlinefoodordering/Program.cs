using OnlineFoodOrdering;

var restaurant = new Restaurant("Spice Route", new[]
{
    new MenuItem("Paneer Tikka", 220m),
    new MenuItem("Veg Biryani", 180m),
    new MenuItem("Gulab Jamun", 90m)
});

var customer = new Customer("Aarav", "aarav@example.com");
var policy = new ThresholdDeliveryChargePolicy(400m, 40m);
var service = new FoodOrderingService(policy);

var order = service.PlaceOrder(customer, restaurant, new[] { "Paneer Tikka", "Veg Biryani" });

Console.WriteLine($"Order: {order.OrderId}");
Console.WriteLine($"Restaurant: {order.Restaurant.Name}");
Console.WriteLine($"Items: {string.Join(", ", order.Items.Select(item => item.Name))}");
Console.WriteLine($"Subtotal: {order.Subtotal:C}");
Console.WriteLine($"Delivery charge: {order.DeliveryCharge:C}");
Console.WriteLine($"Total: {order.TotalAmount:C}");

order.Confirm();
order.StartPreparing();
order.MarkOutForDelivery();
order.Deliver();

Console.WriteLine($"Final status: {order.Status}");

try
{
    order.Cancel();
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"Cancel blocked: {ex.Message}");
}