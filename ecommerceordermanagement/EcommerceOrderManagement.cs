namespace EcommerceOrderManagement;

public sealed class Customer(string name, string email)
{
    public string Name { get; } = !string.IsNullOrWhiteSpace(name) ? name : throw new ArgumentException("Customer name is required.", nameof(name));

    public string Email { get; } = !string.IsNullOrWhiteSpace(email) ? email : throw new ArgumentException("Customer email is required.", nameof(email));
}

public sealed class Product(string name, decimal price)
{
    public string Name { get; } = !string.IsNullOrWhiteSpace(name) ? name : throw new ArgumentException("Product name is required.", nameof(name));

    public decimal Price { get; } = price > 0 ? price : throw new ArgumentOutOfRangeException(nameof(price));
}

public sealed class CartItem(Product product, int quantity)
{
    public Product Product { get; } = product ?? throw new ArgumentNullException(nameof(product));

    public int Quantity { get; private set; } = quantity > 0 ? quantity : throw new ArgumentOutOfRangeException(nameof(quantity));

    public decimal Total => Product.Price * Quantity;

    public void AddQuantity(int amount)
    {
        if (amount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount));
        }

        Quantity += amount;
    }
}

public sealed class Cart(Customer customer)
{
    private readonly List<CartItem> items = new();

    public Customer Customer { get; } = customer ?? throw new ArgumentNullException(nameof(customer));

    public IReadOnlyCollection<CartItem> Items => items.AsReadOnly();

    public void Add(Product product, int quantity)
    {
        var existing = items.FirstOrDefault(item => string.Equals(item.Product.Name, product.Name, StringComparison.OrdinalIgnoreCase));
        if (existing is null)
        {
            items.Add(new CartItem(product, quantity));
            return;
        }

        existing.AddQuantity(quantity);
    }

    public Order Checkout(IPaymentMethod paymentMethod)
    {
        if (items.Count == 0)
        {
            throw new InvalidOperationException("Cart is empty.");
        }

        var order = new Order(Guid.NewGuid(), Customer, items.Select(item => new OrderItem(item.Product, item.Quantity)).ToList());
        paymentMethod.Pay(order);
        return order;
    }
}

public sealed class OrderItem(Product product, int quantity)
{
    public Product Product { get; } = product ?? throw new ArgumentNullException(nameof(product));

    public int Quantity { get; } = quantity > 0 ? quantity : throw new ArgumentOutOfRangeException(nameof(quantity));

    public int ReturnedQuantity { get; private set; }

    public decimal Total => Product.Price * Quantity;

    public decimal RefundFor(int quantity)
    {
        if (quantity <= 0 || quantity > Quantity - ReturnedQuantity)
        {
            throw new InvalidOperationException("Invalid return quantity.");
        }

        ReturnedQuantity += quantity;
        return Product.Price * quantity;
    }
}

public enum OrderStatus
{
    Paid,
    Packed,
    Shipped,
    Delivered,
    Returned
}

public sealed class Order(Guid orderId, Customer customer, IReadOnlyCollection<OrderItem> items)
{
    private readonly List<OrderItem> orderItems = items?.ToList() ?? throw new ArgumentNullException(nameof(items));

    public Guid OrderId { get; } = orderId;

    public Customer Customer { get; } = customer ?? throw new ArgumentNullException(nameof(customer));

    public IReadOnlyCollection<OrderItem> Items => orderItems.AsReadOnly();

    public OrderStatus Status { get; private set; }

    public DateTime? DeliveredAt { get; private set; }

    public decimal TotalAmount => orderItems.Sum(item => item.Total);

    public void MarkPaid() => Status = OrderStatus.Paid;

    public void MarkPacked() => Status = OrderStatus.Packed;

    public void MarkShipped() => Status = OrderStatus.Shipped;

    public void MarkDelivered(DateTime deliveredAt)
    {
        Status = OrderStatus.Delivered;
        DeliveredAt = deliveredAt;
    }

    public decimal ReturnItems(IReadOnlyDictionary<string, int> returnQuantities, DateTime returnDate)
    {
        if (Status != OrderStatus.Delivered)
        {
            throw new InvalidOperationException("Only delivered orders can be returned.");
        }

        if (DeliveredAt is null || returnDate > DeliveredAt.Value.AddDays(7))
        {
            throw new InvalidOperationException("Return window has expired.");
        }

        var refund = 0m;
        foreach (var pair in returnQuantities)
        {
            var item = orderItems.FirstOrDefault(orderItem => string.Equals(orderItem.Product.Name, pair.Key, StringComparison.OrdinalIgnoreCase))
                ?? throw new InvalidOperationException($"Item '{pair.Key}' not found in order.");

            refund += item.RefundFor(pair.Value);
        }

        Status = OrderStatus.Returned;
        return refund;
    }
}

public interface IPaymentMethod
{
    void Pay(Order order);
}

public sealed class CreditCardPayment : IPaymentMethod
{
    public void Pay(Order order) => order.MarkPaid();
}

public sealed class WalletPayment : IPaymentMethod
{
    public void Pay(Order order) => order.MarkPaid();
}

public sealed class CashOnDeliveryPayment : IPaymentMethod
{
    public void Pay(Order order) => order.MarkPaid();
}

public sealed class Warehouse
{
    public void Fulfill(Order order)
    {
        order.MarkPacked();
        order.MarkShipped();
        order.MarkDelivered(DateTime.Today);
    }
}