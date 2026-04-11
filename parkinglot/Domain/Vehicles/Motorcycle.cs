using ParkingLotApp.Domain.Enums;

namespace ParkingLotApp.Domain.Vehicles;

public sealed class Motorcycle : Vehicle
{
    public Motorcycle(string licensePlate) : base(licensePlate)
    {
    }

    public override VehicleSize Size => VehicleSize.Motorcycle;

    public override decimal HourlyRate => 2m;
}