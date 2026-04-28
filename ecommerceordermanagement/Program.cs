using EcommerceOrderManagement;

var customer = new Customer("Aarav", "aarav@example.com");
var cart = new Cart(customer);
var laptop = new Product("Laptop", 55000m);
var mouse = new Product("Mouse", 800m);

cart.Add(laptop, 1);
cart.Add(mouse, 2);

var order = cart.Checkout(new CreditCardPayment());
var warehouse = new Warehouse();
warehouse.Fulfill(order);

Console.WriteLine($"Order: {order.OrderId}");
Console.WriteLine($"Status: {order.Status}");
Console.WriteLine($"Total: {order.TotalAmount:C}");

var refund = order.ReturnItems(new Dictionary<string, int> { ["Mouse"] = 1 }, DateTime.Today.AddDays(3));
Console.WriteLine($"Refund: {refund:C}");