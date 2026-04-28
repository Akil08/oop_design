namespace HotelBooking.Domain;

public sealed class Hotel
{
    private readonly List<Room> rooms;
    private readonly List<Reservation> reservations = new();

    public Hotel(string name, IEnumerable<Room> rooms)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Hotel name is required.", nameof(name));
        }

        this.rooms = rooms?.ToList() ?? throw new ArgumentNullException(nameof(rooms));
        if (this.rooms.Count == 0)
        {
            throw new ArgumentException("Hotel must have at least one room.", nameof(rooms));
        }

        Name = name;
    }

    public string Name { get; }

    public IReadOnlyCollection<Room> Rooms => rooms.AsReadOnly();

    public IReadOnlyCollection<Reservation> Reservations => reservations.AsReadOnly();

    public Reservation BookRooms(Guest guest, DateTime checkIn, DateTime checkOut, IEnumerable<string> roomNumbers)
    {
        ArgumentNullException.ThrowIfNull(guest);

        var requestedRooms = ResolveRooms(roomNumbers);

        foreach (var room in requestedRooms)
        {
            EnsureRoomIsAvailable(room, checkIn, checkOut);
        }

        var reservation = new Reservation(Guid.NewGuid(), guest, checkIn, checkOut, requestedRooms);
        reservations.Add(reservation);
        return reservation;
    }

    public CancellationResult CancelReservation(Guid reservationId, DateTime cancelledAt)
    {
        var reservation = reservations.SingleOrDefault(item => item.ReservationId == reservationId)
            ?? throw new InvalidOperationException("Reservation not found.");

        if (!reservation.IsActive)
        {
            throw new InvalidOperationException("Reservation is already cancelled.");
        }

        var isRefundable = reservation.IsRefundable(cancelledAt);
        reservation.Cancel();

        return new CancellationResult(reservation.ReservationId, isRefundable, isRefundable ? reservation.TotalPrice : 0m);
    }

    private List<Room> ResolveRooms(IEnumerable<string> roomNumbers)
    {
        var numbers = roomNumbers?.Distinct(StringComparer.OrdinalIgnoreCase).ToList()
            ?? throw new ArgumentNullException(nameof(roomNumbers));

        if (numbers.Count == 0)
        {
            throw new ArgumentException("At least one room must be selected.", nameof(roomNumbers));
        }

        var selectedRooms = new List<Room>();
        foreach (var roomNumber in numbers)
        {
            var room = rooms.SingleOrDefault(item => string.Equals(item.RoomNumber, roomNumber, StringComparison.OrdinalIgnoreCase))
                ?? throw new InvalidOperationException($"Room {roomNumber} does not exist.");

            selectedRooms.Add(room);
        }

        return selectedRooms;
    }

    private void EnsureRoomIsAvailable(Room room, DateTime checkIn, DateTime checkOut)
    {
        var isBooked = reservations
            .Where(reservation => reservation.IsActive)
            .Where(reservation => reservation.Rooms.Any(assignedRoom => string.Equals(assignedRoom.RoomNumber, room.RoomNumber, StringComparison.OrdinalIgnoreCase)))
            .Any(reservation => reservation.IsOverlapping(checkIn, checkOut));

        if (isBooked)
        {
            throw new InvalidOperationException($"Room {room.RoomNumber} is already booked for the selected dates.");
        }
    }
}