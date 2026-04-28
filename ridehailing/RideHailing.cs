namespace RideHailing;

public sealed record Location(double X, double Y)
{
    public double DistanceTo(Location other) => Math.Sqrt(Math.Pow(X - other.X, 2) + Math.Pow(Y - other.Y, 2));
}

public sealed class Passenger(string name)
{
    public string Name { get; } = !string.IsNullOrWhiteSpace(name) ? name : throw new ArgumentException("Passenger name is required.", nameof(name));

    public bool IsBusy { get; private set; }

    public void AssignRide() => IsBusy = true;

    public void ReleaseRide() => IsBusy = false;
}

public sealed class Driver(string name, Location location)
{
    public string Name { get; } = !string.IsNullOrWhiteSpace(name) ? name : throw new ArgumentException("Driver name is required.", nameof(name));

    public Location Location { get; private set; } = location ?? throw new ArgumentNullException(nameof(location));

    public bool IsOnline { get; private set; }

    public bool IsBusy { get; private set; }

    public void SetOnline() => IsOnline = true;

    public void SetOffline() => IsOnline = false;

    public void AssignRide() => IsBusy = true;

    public void ReleaseRide() => IsBusy = false;
}

public enum RideStatus
{
    Requested,
    Accepted,
    InProgress,
    Completed,
    Cancelled
}

public sealed class Ride(Guid rideId, Passenger passenger, Driver driver, Location pickup, Location dropOff, decimal fare)
{
    public Guid RideId { get; } = rideId;

    public Passenger Passenger { get; } = passenger ?? throw new ArgumentNullException(nameof(passenger));

    public Driver Driver { get; } = driver ?? throw new ArgumentNullException(nameof(driver));

    public Location Pickup { get; } = pickup ?? throw new ArgumentNullException(nameof(pickup));

    public Location DropOff { get; } = dropOff ?? throw new ArgumentNullException(nameof(dropOff));

    public decimal Fare { get; } = fare;

    public RideStatus Status { get; private set; } = RideStatus.Requested;

    public void Accept()
    {
        EnsureStatus(RideStatus.Requested);
        Status = RideStatus.Accepted;
    }

    public void Start()
    {
        EnsureStatus(RideStatus.Accepted);
        Status = RideStatus.InProgress;
    }

    public void Complete()
    {
        EnsureStatus(RideStatus.InProgress);
        Status = RideStatus.Completed;
    }

    public void Cancel()
    {
        if (Status is RideStatus.Completed or RideStatus.Cancelled)
        {
            throw new InvalidOperationException("Ride cannot be cancelled.");
        }

        Status = RideStatus.Cancelled;
    }

    private void EnsureStatus(RideStatus expected)
    {
        if (Status != expected)
        {
            throw new InvalidOperationException($"Ride must be in {expected} state.");
        }
    }
}

public interface IFareCalculator
{
    decimal CalculateFare(Location pickup, Location dropOff);
}

public sealed class StandardFareCalculator(decimal baseFee) : IFareCalculator
{
    public decimal CalculateFare(Location pickup, Location dropOff) => baseFee + (decimal)pickup.DistanceTo(dropOff) * 10m;
}

public sealed class RideService(IFareCalculator fareCalculator)
{
    private readonly List<Driver> drivers = new();
    private readonly List<Ride> rides = new();

    public void RegisterDriver(Driver driver)
    {
        if (driver is null)
        {
            throw new ArgumentNullException(nameof(driver));
        }

        drivers.Add(driver);
    }

    public Ride RequestRide(Passenger passenger, Location pickup, Location dropOff)
    {
        if (passenger.IsBusy)
        {
            throw new InvalidOperationException("Passenger already has an active ride.");
        }

        var driver = drivers
            .Where(item => item.IsOnline && !item.IsBusy)
            .OrderBy(item => item.Location.DistanceTo(pickup))
            .FirstOrDefault() ?? throw new InvalidOperationException("No nearby driver available.");

        passenger.AssignRide();
        driver.AssignRide();

        var ride = new Ride(Guid.NewGuid(), passenger, driver, pickup, dropOff, fareCalculator.CalculateFare(pickup, dropOff));
        rides.Add(ride);
        return ride;
    }

    public void StartRide(Guid rideId)
    {
        var ride = GetRide(rideId);
        ride.Accept();
        ride.Start();
    }

    public void CompleteRide(Guid rideId)
    {
        var ride = GetRide(rideId);
        ride.Complete();
        ReleaseRide(ride);
    }

    public void CancelRide(Guid rideId)
    {
        var ride = GetRide(rideId);
        ride.Cancel();
        ReleaseRide(ride);
    }

    private Ride GetRide(Guid rideId) =>
        rides.SingleOrDefault(item => item.RideId == rideId) ?? throw new InvalidOperationException("Ride not found.");

    private static void ReleaseRide(Ride ride)
    {
        ride.Passenger.ReleaseRide();
        ride.Driver.ReleaseRide();
    }
}