using ParkingLotApp.Domain.Enums;

namespace ParkingLotApp.Domain.Spots;

public sealed class MotorcycleSpot : ParkingSpot
{
    public MotorcycleSpot(string spotId) : base(spotId, SpotSize.Motorcycle)
    {
    }
}