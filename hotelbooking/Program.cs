using HotelBooking.Domain;

var hotel = new Hotel("Grand Pine Hotel", new Room[]
{
	new SingleRoom("101"),
	new SingleRoom("102"),
	new DoubleRoom("201"),
	new DoubleRoom("202"),
	new SuiteRoom("301")
});

var guest = new Guest("Aarav Sharma", "aarav@example.com");

var checkIn = new DateTime(2026, 4, 20);
var checkOut = new DateTime(2026, 4, 23);

var booking = hotel.BookRooms(guest, checkIn, checkOut, new[] { "101", "301" });

Console.WriteLine($"Booking created: {booking.ReservationId}");
Console.WriteLine($"Guest: {booking.Guest.Name}");
Console.WriteLine($"Rooms: {string.Join(", ", booking.Rooms.Select(room => room.DisplayName))}");
Console.WriteLine($"Stay: {booking.CheckIn:yyyy-MM-dd} to {booking.CheckOut:yyyy-MM-dd}");
Console.WriteLine($"Total price: {booking.TotalPrice:C}");

try
{
	hotel.BookRooms(new Guest("Neha Kapoor", "neha@example.com"), checkIn, checkOut, new[] { "101" });
}
catch (InvalidOperationException ex)
{
	Console.WriteLine($"Double booking prevented: {ex.Message}");
}

var refundableCancellation = hotel.CancelReservation(booking.ReservationId, new DateTime(2026, 4, 18));
Console.WriteLine($"Cancelled {refundableCancellation.ReservationId}. Refundable: {refundableCancellation.IsRefundable}");

var lateBooking = hotel.BookRooms(new Guest("Rohan Mehta", "rohan@example.com"), new DateTime(2026, 5, 1), new DateTime(2026, 5, 3), new[] { "102" });
var nonRefundableCancellation = hotel.CancelReservation(lateBooking.ReservationId, new DateTime(2026, 4, 30, 12, 0, 0));
Console.WriteLine($"Late cancellation refundable: {nonRefundableCancellation.IsRefundable}");
