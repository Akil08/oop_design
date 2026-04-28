namespace InventoryManagementRetail;

public sealed class Product
{
    public Product(string name, string category, decimal price, int stockQuantity, int minimumThreshold)
    {
        Name = !string.IsNullOrWhiteSpace(name) ? name : throw new ArgumentException("Product name is required.", nameof(name));
        Category = !string.IsNullOrWhiteSpace(category) ? category : throw new ArgumentException("Category is required.", nameof(category));
        Price = price > 0 ? price : throw new ArgumentOutOfRangeException(nameof(price));
        StockQuantity = stockQuantity >= 0 ? stockQuantity : throw new ArgumentOutOfRangeException(nameof(stockQuantity));
        MinimumThreshold = minimumThreshold >= 0 ? minimumThreshold : throw new ArgumentOutOfRangeException(nameof(minimumThreshold));
    }

    public string Name { get; private set; }

    public string Category { get; private set; }

    public decimal Price { get; private set; }

    public int StockQuantity { get; private set; }

    public int MinimumThreshold { get; private set; }

    public bool IsDiscontinued { get; private set; }

    public void Update(string category, decimal price, int minimumThreshold)
    {
        Category = !string.IsNullOrWhiteSpace(category) ? category : throw new ArgumentException("Category is required.", nameof(category));
        Price = price > 0 ? price : throw new ArgumentOutOfRangeException(nameof(price));
        MinimumThreshold = minimumThreshold >= 0 ? minimumThreshold : throw new ArgumentOutOfRangeException(nameof(minimumThreshold));
    }

    public void AddStock(int quantity)
    {
        if (quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity));
        }

        StockQuantity += quantity;
    }

    public void ReduceStock(int quantity)
    {
        if (quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity));
        }

        if (IsDiscontinued)
        {
            throw new InvalidOperationException("Discontinued products cannot be sold.");
        }

        if (StockQuantity < quantity)
        {
            throw new InvalidOperationException("Not enough stock.");
        }

        StockQuantity -= quantity;
    }

    public void Discontinue() => IsDiscontinued = true;
}

public sealed record PurchaseOrder(string ProductName, int Quantity);

public sealed record RestockAlert(string ProductName, int CurrentStock, int MinimumThreshold)
{
    public override string ToString() => $"Restock alert for {ProductName}: stock {CurrentStock}, threshold {MinimumThreshold}";
}

public sealed class InventoryStore
{
    private readonly Dictionary<string, Product> products = new(StringComparer.OrdinalIgnoreCase);

    public void AddOrUpdateProduct(Product product)
    {
        if (products.TryGetValue(product.Name, out var existing))
        {
            existing.Update(product.Category, product.Price, product.MinimumThreshold);
            existing.AddStock(product.StockQuantity);
            return;
        }

        products[product.Name] = product;
    }

    public Product GetProduct(string name) =>
        products.TryGetValue(name, out var product) ? product : throw new InvalidOperationException("Product not found.");

    public RestockAlert? Sell(string productName, int quantity)
    {
        var product = GetProduct(productName);
        product.ReduceStock(quantity);

        return product.StockQuantity < product.MinimumThreshold
            ? new RestockAlert(product.Name, product.StockQuantity, product.MinimumThreshold)
            : null;
    }

    public void ReceiveStock(PurchaseOrder order)
    {
        var product = GetProduct(order.ProductName);
        product.AddStock(order.Quantity);
    }

    public void Discontinue(string productName)
    {
        GetProduct(productName).Discontinue();
    }
}