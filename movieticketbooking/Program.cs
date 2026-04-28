using MovieTicketBooking;

var chain = new CinemaChain(new[]
{
    new Theatre("Pune Central", new[]
    {
        new Screen(1, 3, 4)
    }),
    new Theatre("Mumbai West", new[]
    {
        new Screen(1, 2, 3)
    })
});

var movie = new Movie("Interstellar Nights");
var show = chain.AddShow("Pune Central", 1, movie, new DateTime(2026, 5, 1, 18, 30, 0));
var service = new BookingService();

var booking = service.CreateBooking(show, "Aarav", new[] { "A1", "A2", "B1" }, DateTime.Now);

Console.WriteLine($"Booking: {booking.BookingId}");
Console.WriteLine($"Seats: {string.Join(", ", booking.Seats.Select(seat => seat.SeatNumber))}");
Console.WriteLine($"Amount: {booking.TotalAmount:C}");

service.ConfirmPayment(booking.BookingId, DateTime.Now.AddMinutes(2));
booking.CancelSeats(new[] { "A2" });

Console.WriteLine($"Remaining seats after partial cancellation: {string.Join(", ", booking.Seats.Select(seat => seat.SeatNumber))}");
Console.WriteLine($"Final status: {booking.Status}");