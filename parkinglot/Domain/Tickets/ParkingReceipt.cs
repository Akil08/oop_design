namespace ParkingLotApp.Domain.Tickets;

public sealed class ParkingReceipt
{
    public ParkingReceipt(Guid ticketId, string licensePlate, string spotId, int floorNumber, DateTimeOffset entryTime, DateTimeOffset exitTime, decimal amount)
    {
        TicketId = ticketId;
        LicensePlate = licensePlate;
        SpotId = spotId;
        FloorNumber = floorNumber;
        EntryTime = entryTime;
        ExitTime = exitTime;
        Amount = amount;
    }

    public Guid TicketId { get; }

    public string LicensePlate { get; }

    public string SpotId { get; }

    public int FloorNumber { get; }

    public DateTimeOffset EntryTime { get; }

    public DateTimeOffset ExitTime { get; }

    public decimal Amount { get; }

    public override string ToString() => $"Receipt: {LicensePlate} | Floor {FloorNumber} Spot {SpotId} | {EntryTime:u} -> {ExitTime:u} | Fee {Amount:C}";
}