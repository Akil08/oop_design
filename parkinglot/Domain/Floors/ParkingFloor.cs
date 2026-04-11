using ParkingLotApp.Domain.Spots;
using ParkingLotApp.Domain.Vehicles;

namespace ParkingLotApp.Domain.Floors;

public sealed class ParkingFloor
{
    private readonly List<ParkingSpot> _spots;

    public ParkingFloor(int floorNumber, IEnumerable<ParkingSpot> spots)
    {
        FloorNumber = floorNumber;
        _spots = spots.ToList();
    }

    public int FloorNumber { get; }

    public IReadOnlyCollection<ParkingSpot> Spots => _spots.AsReadOnly();

    public ParkingSpot? FindSpotFor(Vehicle vehicle) => _spots.FirstOrDefault(spot => spot.IsAvailable && spot.CanFit(vehicle));

    public int GetAvailableSpotCount() => _spots.Count(spot => spot.IsAvailable);
}