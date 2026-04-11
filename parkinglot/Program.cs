using ParkingLotApp.Domain;
using ParkingLotApp.Domain.Floors;
using ParkingLotApp.Domain.Spots;
using ParkingLotApp.Domain.Vehicles;
using ParkingLotApp.Gates;
using ParkingLotApp.Managers;

var lot = new ParkingLot("Central Parking", new[]
{
    new ParkingFloor(1, new ParkingSpot[]
    {
        new MotorcycleSpot("1A"),
        new CompactSpot("1B"),
        new CompactSpot("1C"),
        new LargeSpot("1D")
    }),
    new ParkingFloor(2, new[]
    {
        new MotorcycleSpot("2A"),
        new CompactSpot("2B"),
        new LargeSpot("2C"),
        new LargeSpot("2D")
    })
});

var manager = new ParkingLotManager(lot);
var entryGate = new EntryGate(manager);
var exitGate = new ExitGate(manager);

var motorcycleTicket = entryGate.Enter(new Motorcycle("M-101"));
var carTicket = entryGate.Enter(new Car("C-202"));
var truckTicket = entryGate.Enter(new Truck("T-303"));

Console.WriteLine(manager.GetAvailabilityReport());

carTicket.EntryTime = DateTimeOffset.UtcNow.AddHours(-2.5);
var carReceipt = exitGate.Exit(carTicket.TicketId);

motorcycleTicket.EntryTime = DateTimeOffset.UtcNow.AddHours(-1.25);
var motorcycleReceipt = exitGate.Exit(motorcycleTicket.TicketId);

Console.WriteLine(carReceipt);
Console.WriteLine(motorcycleReceipt);
Console.WriteLine(manager.GetAvailabilityReport());