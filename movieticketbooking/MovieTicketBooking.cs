namespace MovieTicketBooking;

public enum SeatCategory
{
    Regular,
    Premium,
    Recliner
}

public enum BookingStatus
{
    Held,
    Confirmed,
    Cancelled,
    Expired
}

public sealed class Movie(string title)
{
    public string Title { get; } = !string.IsNullOrWhiteSpace(title) ? title : throw new ArgumentException("Movie title is required.", nameof(title));
}

public sealed class Seat(string seatNumber, SeatCategory category)
{
    private static readonly Dictionary<SeatCategory, decimal> Prices = new()
    {
        [SeatCategory.Regular] = 150m,
        [SeatCategory.Premium] = 220m,
        [SeatCategory.Recliner] = 300m
    };

    public string SeatNumber { get; } = !string.IsNullOrWhiteSpace(seatNumber) ? seatNumber : throw new ArgumentException("Seat number is required.", nameof(seatNumber));

    public SeatCategory Category { get; } = category;

    public decimal Price => Prices[Category];

    public bool IsBooked { get; private set; }

    public bool IsHeld { get; private set; }

    public Guid? HeldByBookingId { get; private set; }

    public void Hold(Guid bookingId)
    {
        if (IsBooked || IsHeld)
        {
            throw new InvalidOperationException($"Seat {SeatNumber} is not available.");
        }

        IsHeld = true;
        HeldByBookingId = bookingId;
    }

    public void Confirm(Guid bookingId)
    {
        if (!IsHeld || HeldByBookingId != bookingId)
        {
            throw new InvalidOperationException($"Seat {SeatNumber} is not held by this booking.");
        }

        IsHeld = false;
        IsBooked = true;
        HeldByBookingId = null;
    }

    public void Release(Guid bookingId)
    {
        if (IsHeld && HeldByBookingId == bookingId)
        {
            IsHeld = false;
            HeldByBookingId = null;
        }
    }

    public void CancelBooking()
    {
        IsBooked = false;
    }
}

public sealed class Screen
{
    private readonly List<Seat> seats = new();

    public Screen(int screenNumber, int rows, int seatsPerRow)
    {
        ScreenNumber = screenNumber;
        for (var row = 1; row <= rows; row++)
        {
            for (var seatIndex = 1; seatIndex <= seatsPerRow; seatIndex++)
            {
                var category = seatIndex == seatsPerRow ? SeatCategory.Recliner : seatIndex == seatsPerRow - 1 ? SeatCategory.Premium : SeatCategory.Regular;
                seats.Add(new Seat($"{(char)('A' + row - 1)}{seatIndex}", category));
            }
        }
    }

    public int ScreenNumber { get; }

    public IReadOnlyCollection<Seat> Seats => seats.AsReadOnly();

    public Seat GetSeat(string seatNumber) =>
        seats.FirstOrDefault(seat => string.Equals(seat.SeatNumber, seatNumber, StringComparison.OrdinalIgnoreCase))
        ?? throw new InvalidOperationException($"Seat {seatNumber} not found.");
}

public sealed class Theatre(string name, IReadOnlyCollection<Screen> screens)
{
    public string Name { get; } = !string.IsNullOrWhiteSpace(name) ? name : throw new ArgumentException("Theatre name is required.", nameof(name));

    public IReadOnlyCollection<Screen> Screens { get; } = screens ?? throw new ArgumentNullException(nameof(screens));
}

public sealed class CinemaChain(IEnumerable<Theatre> theatres)
{
    private readonly List<Theatre> theatres = theatres?.ToList() ?? throw new ArgumentNullException(nameof(theatres));
    private readonly List<Show> shows = new();

    public IReadOnlyCollection<Theatre> Theatres => theatres.AsReadOnly();

    public Show AddShow(string theatreName, int screenNumber, Movie movie, DateTime startsAt)
    {
        var theatre = theatres.FirstOrDefault(item => string.Equals(item.Name, theatreName, StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidOperationException("Theatre not found.");

        var screen = theatre.Screens.FirstOrDefault(item => item.ScreenNumber == screenNumber)
            ?? throw new InvalidOperationException("Screen not found.");

        var show = new Show(Guid.NewGuid(), movie, screen, startsAt);
        shows.Add(show);
        return show;
    }
}

public sealed class Show(Guid showId, Movie movie, Screen screen, DateTime startsAt)
{
    public Guid ShowId { get; } = showId;

    public Movie Movie { get; } = movie ?? throw new ArgumentNullException(nameof(movie));

    public Screen Screen { get; } = screen ?? throw new ArgumentNullException(nameof(screen));

    public DateTime StartsAt { get; } = startsAt;

    public decimal TotalFor(IEnumerable<string> seatNumbers)
    {
        return seatNumbers.Select(Screen.GetSeat).Sum(seat => seat.Price);
    }
}

public sealed class Booking
{
    private readonly List<Seat> seats;

    public Booking(Guid bookingId, Show show, string customerName, IReadOnlyCollection<Seat> seats, DateTime expiresAt)
    {
        BookingId = bookingId;
        Show = show ?? throw new ArgumentNullException(nameof(show));
        CustomerName = !string.IsNullOrWhiteSpace(customerName) ? customerName : throw new ArgumentException("Customer name is required.", nameof(customerName));
        this.seats = seats?.ToList() ?? throw new ArgumentNullException(nameof(seats));
        ExpiresAt = expiresAt;
        Status = BookingStatus.Held;
    }

    public Guid BookingId { get; }

    public Show Show { get; }

    public string CustomerName { get; }

    public IReadOnlyCollection<Seat> Seats => seats.AsReadOnly();

    public DateTime ExpiresAt { get; }

    public BookingStatus Status { get; private set; }

    public decimal TotalAmount => seats.Sum(seat => seat.Price);

    public void Confirm()
    {
        if (Status != BookingStatus.Held)
        {
            throw new InvalidOperationException("Booking cannot be confirmed.");
        }

        foreach (var seat in seats)
        {
            seat.Confirm(BookingId);
        }

        Status = BookingStatus.Confirmed;
    }

    public void ExpireIfNeeded(DateTime now)
    {
        if (Status == BookingStatus.Held && now >= ExpiresAt)
        {
            ReleaseAll();
            Status = BookingStatus.Expired;
        }
    }

    public void CancelSeats(IEnumerable<string> seatNumbers)
    {
        var selectedNumbers = seatNumbers?.ToList() ?? throw new ArgumentNullException(nameof(seatNumbers));
        if (selectedNumbers.Count == 0)
        {
            throw new ArgumentException("At least one seat is required.", nameof(seatNumbers));
        }

        var remainingSeats = seats.Where(seat => !selectedNumbers.Contains(seat.SeatNumber, StringComparer.OrdinalIgnoreCase)).ToList();
        var cancelledSeats = seats.Except(remainingSeats).ToList();

        foreach (var seat in cancelledSeats)
        {
            seat.CancelBooking();
        }

        seats.Clear();
        seats.AddRange(remainingSeats);

        if (seats.Count == 0)
        {
            Status = BookingStatus.Cancelled;
        }
    }

    private void ReleaseAll()
    {
        foreach (var seat in seats)
        {
            seat.Release(BookingId);
        }
    }
}

public sealed class BookingService
{
    private readonly List<Booking> bookings = new();

    public Booking CreateBooking(Show show, string customerName, IEnumerable<string> seatNumbers, DateTime now)
    {
        var requestedSeatNumbers = seatNumbers?.ToList() ?? throw new ArgumentNullException(nameof(seatNumbers));
        if (requestedSeatNumbers.Count == 0)
        {
            throw new ArgumentException("At least one seat is required.", nameof(seatNumbers));
        }

        ReleaseExpiredBookings(now);

        var seats = requestedSeatNumbers.Select(show.Screen.GetSeat).ToList();
        foreach (var seat in seats)
        {
            seat.Hold(Guid.NewGuid());
        }

        var bookingId = Guid.NewGuid();
        foreach (var seat in seats)
        {
            seat.Release(seat.HeldByBookingId ?? bookingId);
            seat.Hold(bookingId);
        }

        var booking = new Booking(bookingId, show, customerName, seats, now.AddMinutes(10));
        bookings.Add(booking);
        return booking;
    }

    public void ConfirmPayment(Guid bookingId, DateTime now)
    {
        ReleaseExpiredBookings(now);
        var booking = bookings.SingleOrDefault(item => item.BookingId == bookingId)
            ?? throw new InvalidOperationException("Booking not found.");

        booking.Confirm();
    }

    public void ReleaseExpiredBookings(DateTime now)
    {
        foreach (var booking in bookings)
        {
            booking.ExpireIfNeeded(now);
        }
    }
}