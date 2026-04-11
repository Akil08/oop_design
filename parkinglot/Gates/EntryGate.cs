using ParkingLotApp.Domain.Tickets;
using ParkingLotApp.Domain.Vehicles;
using ParkingLotApp.Managers;

namespace ParkingLotApp.Gates;

public sealed class EntryGate
{
    private readonly ParkingLotManager _manager;

    public EntryGate(ParkingLotManager manager)
    {
        _manager = manager;
    }

    public ParkingTicket Enter(Vehicle vehicle) => _manager.AdmitVehicle(vehicle);
}