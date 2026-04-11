using ParkingLotApp.Domain.Enums;

namespace ParkingLotApp.Domain.Vehicles;

public sealed class Car : Vehicle
{
    public Car(string licensePlate) : base(licensePlate)
    {
    }

    public override VehicleSize Size => VehicleSize.Car;

    public override decimal HourlyRate => 4m;
}