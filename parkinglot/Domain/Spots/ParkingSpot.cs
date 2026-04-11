using ParkingLotApp.Domain.Enums;
using ParkingLotApp.Domain.Vehicles;

namespace ParkingLotApp.Domain.Spots;

public abstract class ParkingSpot
{
    protected ParkingSpot(string spotId, SpotSize size)
    {
        SpotId = spotId;
        Size = size;
    }

    public string SpotId { get; }

    public SpotSize Size { get; }

    public Vehicle? CurrentVehicle { get; private set; }

    public bool IsAvailable => CurrentVehicle is null;

    public bool CanFit(Vehicle vehicle) => vehicle.Size <= (VehicleSize)Size;

    public void Park(Vehicle vehicle)
    {
        if (!IsAvailable)
        {
            throw new InvalidOperationException("Spot is occupied.");
        }

        if (!CanFit(vehicle))
        {
            throw new InvalidOperationException("Vehicle does not fit in this spot.");
        }

        CurrentVehicle = vehicle;
    }

    public void RemoveVehicle()
    {
        CurrentVehicle = null;
    }
}