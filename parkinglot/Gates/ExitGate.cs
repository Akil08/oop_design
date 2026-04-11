using ParkingLotApp.Domain.Tickets;
using ParkingLotApp.Managers;

namespace ParkingLotApp.Gates;

public sealed class ExitGate
{
    private readonly ParkingLotManager _manager;

    public ExitGate(ParkingLotManager manager)
    {
        _manager = manager;
    }

    public ParkingReceipt Exit(Guid ticketId) => _manager.ProcessExit(ticketId, DateTimeOffset.UtcNow);
}