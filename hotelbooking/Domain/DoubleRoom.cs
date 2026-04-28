namespace HotelBooking.Domain;

public sealed class DoubleRoom : Room
{
    public DoubleRoom(string roomNumber)
        : base(roomNumber, 150m)
    {
    }

    public override string RoomType => "Double";
}