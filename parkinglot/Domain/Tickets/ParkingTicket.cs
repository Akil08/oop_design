using ParkingLotApp.Domain.Vehicles;

namespace ParkingLotApp.Domain.Tickets;

public sealed class ParkingTicket
{
    public ParkingTicket(Guid ticketId, Vehicle vehicle, string spotId, int floorNumber, DateTimeOffset entryTime)
    {
        TicketId = ticketId;
        Vehicle = vehicle;
        SpotId = spotId;
        FloorNumber = floorNumber;
        EntryTime = entryTime;
    }

    public Guid TicketId { get; }

    public Vehicle Vehicle { get; }

    public string SpotId { get; }

    public int FloorNumber { get; }

    public DateTimeOffset EntryTime { get; set; }
}