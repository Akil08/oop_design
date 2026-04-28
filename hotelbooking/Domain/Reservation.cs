namespace HotelBooking.Domain;

public sealed class Reservation
{
    private readonly List<Room> rooms;

    public Reservation(Guid reservationId, Guest guest, DateTime checkIn, DateTime checkOut, IReadOnlyCollection<Room> rooms)
    {
        ArgumentNullException.ThrowIfNull(guest);
        ArgumentNullException.ThrowIfNull(rooms);

        if (checkOut <= checkIn)
        {
            throw new ArgumentException("Check-out must be after check-in.");
        }

        if (rooms.Count == 0)
        {
            throw new ArgumentException("At least one room is required.", nameof(rooms));
        }

        ReservationId = reservationId;
        Guest = guest;
        CheckIn = checkIn.Date;
        CheckOut = checkOut.Date;
        this.rooms = rooms.ToList();
        Status = ReservationStatus.Active;
    }

    public Guid ReservationId { get; }

    public Guest Guest { get; }

    public DateTime CheckIn { get; }

    public DateTime CheckOut { get; }

    public IReadOnlyCollection<Room> Rooms => rooms.AsReadOnly();

    public ReservationStatus Status { get; private set; }

    public bool IsActive => Status == ReservationStatus.Active;

    public int Nights => (int)(CheckOut - CheckIn).TotalDays;

    public decimal TotalPrice => rooms.Sum(room => room.NightlyRate * Nights);

    public bool IsOverlapping(DateTime requestedCheckIn, DateTime requestedCheckOut)
    {
        return requestedCheckIn.Date < CheckOut && requestedCheckOut.Date > CheckIn;
    }

    public void Cancel()
    {
        Status = ReservationStatus.Cancelled;
    }

    public bool IsRefundable(DateTime cancelledAt)
    {
        var refundCutoff = CheckIn.AddHours(-24);
        return cancelledAt < refundCutoff;
    }
}