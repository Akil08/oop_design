namespace LibraryManagement.Models;

public enum ReservationStatus { Waiting, Ready, Cancelled }

public class Reservation
{
    public int ReservationId { get; init; }
    public Book Book { get; init; }
    public Member Member { get; init; }
    public DateTime ReservationDate { get; init; }
    public ReservationStatus Status { get; private set; } = ReservationStatus.Waiting;

    public Reservation(int id, Book book, Member member)
    {
        ReservationId = id;
        Book = book;
        Member = member;
        ReservationDate = DateTime.Today;
    }

    public void MarkAsReady() => Status = ReservationStatus.Ready;
    public void Cancel() => Status = ReservationStatus.Cancelled;
    public bool IsActive() => Status == ReservationStatus.Waiting || Status == ReservationStatus.Ready;
}