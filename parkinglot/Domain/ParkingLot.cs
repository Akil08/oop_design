using ParkingLotApp.Domain.Floors;
using ParkingLotApp.Domain.Spots;
using ParkingLotApp.Domain.Tickets;
using ParkingLotApp.Domain.Vehicles;

namespace ParkingLotApp.Domain;

public sealed class ParkingLot
{
    private readonly List<ParkingFloor> _floors;
    private readonly Dictionary<Guid, ParkingTicket> _tickets = new();
    private readonly Dictionary<Guid, ParkingSpot> _assignedSpots = new();

    public ParkingLot(string name, IEnumerable<ParkingFloor> floors)
    {
        Name = name;
        _floors = floors.ToList();
    }

    public string Name { get; }

    public ParkingTicket ParkVehicle(Vehicle vehicle)
    {
        foreach (var floor in _floors)
        {
            var spot = floor.FindSpotFor(vehicle);

            if (spot is null)
            {
                continue;
            }

            spot.Park(vehicle);
            var ticket = new ParkingTicket(Guid.NewGuid(), vehicle, spot.SpotId, floor.FloorNumber, DateTimeOffset.UtcNow);
            _tickets[ticket.TicketId] = ticket;
            _assignedSpots[ticket.TicketId] = spot;
            return ticket;
        }

        throw new InvalidOperationException("No available spot for this vehicle.");
    }

    public ParkingReceipt ExitVehicle(Guid ticketId, DateTimeOffset exitTime)
    {
        if (!_tickets.TryGetValue(ticketId, out var ticket))
        {
            throw new InvalidOperationException("Invalid ticket.");
        }

        var parkedDuration = exitTime - ticket.EntryTime;
        var billableHours = Math.Max(1, Math.Ceiling(parkedDuration.TotalHours));
        var amount = (decimal)billableHours * ticket.Vehicle.HourlyRate;

        if (!_assignedSpots.TryGetValue(ticketId, out var spot))
        {
            throw new InvalidOperationException("Parking spot not found.");
        }

        spot.RemoveVehicle();
        _tickets.Remove(ticketId);
        _assignedSpots.Remove(ticketId);

        return new ParkingReceipt(ticket.TicketId, ticket.Vehicle.LicensePlate, ticket.SpotId, ticket.FloorNumber, ticket.EntryTime, exitTime, amount);
    }

    public string GetAvailabilityReport()
    {
        var lines = _floors.Select(floor => $"Floor {floor.FloorNumber}: {floor.GetAvailableSpotCount()} spots available");
        var total = _floors.Sum(floor => floor.GetAvailableSpotCount());
        return $"{Name} | Total available: {total}{Environment.NewLine}{string.Join(Environment.NewLine, lines)}";
    }
}