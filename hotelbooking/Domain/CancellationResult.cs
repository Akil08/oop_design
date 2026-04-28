namespace HotelBooking.Domain;

public sealed record CancellationResult(Guid ReservationId, bool IsRefundable, decimal RefundAmount);