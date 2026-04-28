namespace HotelBooking.Domain;

public sealed class SuiteRoom : Room
{
    public SuiteRoom(string roomNumber)
        : base(roomNumber, 250m)
    {
    }

    public override string RoomType => "Suite";
}