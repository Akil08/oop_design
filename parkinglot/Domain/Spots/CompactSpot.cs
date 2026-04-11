using ParkingLotApp.Domain.Enums;

namespace ParkingLotApp.Domain.Spots;

public sealed class CompactSpot : ParkingSpot
{
    public CompactSpot(string spotId) : base(spotId, SpotSize.Compact)
    {
    }
}