namespace HotelBooking.Domain;

public abstract class Room
{
    protected Room(string roomNumber, decimal nightlyRate)
    {
        if (string.IsNullOrWhiteSpace(roomNumber))
        {
            throw new ArgumentException("Room number is required.", nameof(roomNumber));
        }

        if (nightlyRate <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(nightlyRate), "Nightly rate must be positive.");
        }

        RoomNumber = roomNumber;
        NightlyRate = nightlyRate;
    }

    public string RoomNumber { get; }

    public decimal NightlyRate { get; }

    public abstract string RoomType { get; }

    public string DisplayName => $"{RoomType} {RoomNumber}";
}