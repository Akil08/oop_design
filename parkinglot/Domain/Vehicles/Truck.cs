using ParkingLotApp.Domain.Enums;

namespace ParkingLotApp.Domain.Vehicles;

public sealed class Truck : Vehicle
{
    public Truck(string licensePlate) : base(licensePlate)
    {
    }

    public override VehicleSize Size => VehicleSize.Truck;

    public override decimal HourlyRate => 6m;
}