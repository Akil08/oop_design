using RideHailing;

var service = new RideService(new StandardFareCalculator(30m));

var driver1 = new Driver("Driver-1", new Location(1, 1));
var driver2 = new Driver("Driver-2", new Location(9, 9));
driver1.SetOnline();
driver2.SetOnline();

service.RegisterDriver(driver1);
service.RegisterDriver(driver2);

var passenger = new Passenger("Passenger-1");
var ride = service.RequestRide(passenger, new Location(2, 2), new Location(8, 8));

Console.WriteLine($"Ride: {ride.RideId}");
Console.WriteLine($"Driver: {ride.Driver.Name}");
Console.WriteLine($"Fare: {ride.Fare:C}");

service.StartRide(ride.RideId);
service.CompleteRide(ride.RideId);

Console.WriteLine($"Ride status: {ride.Status}");