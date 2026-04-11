using ParkingLotApp.Domain;
using ParkingLotApp.Domain.Tickets;
using ParkingLotApp.Domain.Vehicles;

namespace ParkingLotApp.Managers;

public sealed class ParkingLotManager
{
    private readonly ParkingLot _parkingLot;

    public ParkingLotManager(ParkingLot parkingLot)
    {
        _parkingLot = parkingLot;
    }

    public ParkingTicket AdmitVehicle(Vehicle vehicle) => _parkingLot.ParkVehicle(vehicle);

    public ParkingReceipt ProcessExit(Guid ticketId, DateTimeOffset exitTime) => _parkingLot.ExitVehicle(ticketId, exitTime);

    public string GetAvailabilityReport() => _parkingLot.GetAvailabilityReport();
}