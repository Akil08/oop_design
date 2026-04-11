using ParkingLotApp.Domain.Enums;

namespace ParkingLotApp.Domain.Vehicles;

public abstract class Vehicle
{
    protected Vehicle(string licensePlate)
    {
        LicensePlate = licensePlate;
    }

    public string LicensePlate { get; }

    public abstract VehicleSize Size { get; }

    public abstract decimal HourlyRate { get; }
}