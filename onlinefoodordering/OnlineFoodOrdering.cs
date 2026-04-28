namespace OnlineFoodOrdering;

public sealed class Customer(string name, string email)
{
    public string Name { get; } = !string.IsNullOrWhiteSpace(name) ? name : throw new ArgumentException("Customer name is required.", nameof(name));

    public string Email { get; } = !string.IsNullOrWhiteSpace(email) ? email : throw new ArgumentException("Customer email is required.", nameof(email));
}

public sealed class MenuItem(string name, decimal price, bool isAvailable = true)
{
    public string Name { get; } = !string.IsNullOrWhiteSpace(name) ? name : throw new ArgumentException("Item name is required.", nameof(name));

    public decimal Price { get; } = price > 0 ? price : throw new ArgumentOutOfRangeException(nameof(price));

    public bool IsAvailable { get; private set; } = isAvailable;

    public void SetAvailability(bool isAvailable) => IsAvailable = isAvailable;
}

public sealed class Restaurant
{
    private readonly List<MenuItem> menuItems;

    public Restaurant(string name, IEnumerable<MenuItem> items)
    {
        Name = !string.IsNullOrWhiteSpace(name) ? name : throw new ArgumentException("Restaurant name is required.", nameof(name));
        menuItems = items?.ToList() ?? throw new ArgumentNullException(nameof(items));
    }

    public string Name { get; }

    public IReadOnlyCollection<MenuItem> MenuItems => menuItems.AsReadOnly();

    public MenuItem GetItem(string itemName) =>
        menuItems.FirstOrDefault(item => string.Equals(item.Name, itemName, StringComparison.OrdinalIgnoreCase))
        ?? throw new InvalidOperationException($"Item '{itemName}' is not on the menu.");
}

public enum OrderStatus
{
    Placed,
    Confirmed,
    Preparing,
    OutForDelivery,
    Delivered,
    Cancelled
}

public interface IDeliveryChargePolicy
{
    decimal CalculateCharge(decimal subtotal);
}

public sealed class ThresholdDeliveryChargePolicy(decimal freeDeliveryThreshold, decimal deliveryCharge) : IDeliveryChargePolicy
{
    public decimal CalculateCharge(decimal subtotal) => subtotal >= freeDeliveryThreshold ? 0m : deliveryCharge;
}

public sealed class Order
{
    private readonly List<MenuItem> items;

    public Order(Guid orderId, Customer customer, Restaurant restaurant, IReadOnlyCollection<MenuItem> items, IDeliveryChargePolicy deliveryChargePolicy)
    {
        OrderId = orderId;
        Customer = customer ?? throw new ArgumentNullException(nameof(customer));
        Restaurant = restaurant ?? throw new ArgumentNullException(nameof(restaurant));
        this.items = items?.ToList() ?? throw new ArgumentNullException(nameof(items));

        if (this.items.Count == 0)
        {
            throw new ArgumentException("At least one item is required.", nameof(items));
        }

        Subtotal = this.items.Sum(item => item.Price);
        DeliveryCharge = deliveryChargePolicy.CalculateCharge(Subtotal);
        Status = OrderStatus.Placed;
    }

    public Guid OrderId { get; }

    public Customer Customer { get; }

    public Restaurant Restaurant { get; }

    public IReadOnlyCollection<MenuItem> Items => items.AsReadOnly();

    public decimal Subtotal { get; }

    public decimal DeliveryCharge { get; }

    public decimal TotalAmount => Subtotal + DeliveryCharge;

    public OrderStatus Status { get; private set; }

    public void Confirm()
    {
        EnsureStatus(OrderStatus.Placed);
        Status = OrderStatus.Confirmed;
    }

    public void StartPreparing()
    {
        EnsureStatus(OrderStatus.Confirmed);
        Status = OrderStatus.Preparing;
    }

    public void MarkOutForDelivery()
    {
        EnsureStatus(OrderStatus.Preparing);
        Status = OrderStatus.OutForDelivery;
    }

    public void Deliver()
    {
        EnsureStatus(OrderStatus.OutForDelivery);
        Status = OrderStatus.Delivered;
    }

    public void Cancel()
    {
        if (Status != OrderStatus.Placed)
        {
            throw new InvalidOperationException("Order can only be cancelled before it is confirmed.");
        }

        Status = OrderStatus.Cancelled;
    }

    private void EnsureStatus(OrderStatus expectedStatus)
    {
        if (Status != expectedStatus)
        {
            throw new InvalidOperationException($"Order must be in {expectedStatus} state.");
        }
    }
}

public sealed class FoodOrderingService(IDeliveryChargePolicy deliveryChargePolicy)
{
    public Order PlaceOrder(Customer customer, Restaurant restaurant, IEnumerable<string> itemNames)
    {
        var names = itemNames?.ToList() ?? throw new ArgumentNullException(nameof(itemNames));
        if (names.Count == 0)
        {
            throw new ArgumentException("At least one item is required.", nameof(itemNames));
        }

        var items = names.Select(restaurant.GetItem).ToList();
        if (items.Any(item => !item.IsAvailable))
        {
            throw new InvalidOperationException("One or more items are currently unavailable.");
        }

        return new Order(Guid.NewGuid(), customer, restaurant, items, deliveryChargePolicy);
    }
}