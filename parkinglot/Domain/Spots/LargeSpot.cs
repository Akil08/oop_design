using ParkingLotApp.Domain.Enums;

namespace ParkingLotApp.Domain.Spots;

public sealed class LargeSpot : ParkingSpot
{
    public LargeSpot(string spotId) : base(spotId, SpotSize.Large)
    {
    }
}