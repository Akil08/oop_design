namespace HotelBooking.Domain;

public sealed class SingleRoom : Room
{
    public SingleRoom(string roomNumber)
        : base(roomNumber, 100m)
    {
    }

    public override string RoomType => "Single";
}