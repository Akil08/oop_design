using System;
using System.Collections.Generic;
using System.Linq;
using LibraryManagement.Models;

namespace LibraryManagement.Services;

public class LibraryManager
{
    // In-memory "database"
    private readonly List<Book> _books = new();
    private readonly List<Member> _members = new();
    private readonly List<Loan> _loans = new();
    private readonly List<Reservation> _reservations = new();

    // Auto-incrementing IDs
    private int _nextLoanId = 1;
    private int _nextReservationId = 1;

    private static LibraryManager? _instance;
        private static readonly object _lock = new();

        private LibraryManager() { }

        public static LibraryManager GetInstance()
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                        _instance = new LibraryManager();
                }
            }
            return _instance;
        }

    // --- Data Seeding ---
    public Book AddBook(string isbn, string title, string author, int initialCopies)
    {
        if (initialCopies <= 0) throw new ArgumentException("Must add at least 1 copy.");
        if (_books.Any(b => b.Isbn == isbn)) throw new InvalidOperationException("ISBN already exists.");

        var book = new Book(isbn, title, author, initialCopies);
        _books.Add(book);
        return book;
    }

    public Member AddMember(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name cannot be empty.");
        var member = new Member(_members.Count + 1, name);
        _members.Add(member);
        return member;
    }

    // --- Lookups (for console menu) ---
    public Book? FindBookByIsbn(string isbn) => _books.FirstOrDefault(b => b.Isbn == isbn);
    public Member? FindMemberById(int id) => _members.FirstOrDefault(m => m.Id == id);

    // --- Borrow Rule: Max 3 books, 14-day limit handled in Loan ---
    public Loan BorrowBook(Member member, Book book)
    {
        if (!member.CanBorrowMore())
            throw new InvalidOperationException("Member already has 3 active loans.");

        var copy = book.GetAvailableCopy();
        if (copy == null)
            throw new InvalidOperationException("No copies available. Please reserve instead.");

        var loan = new Loan(_nextLoanId++, copy, member);
        copy.Borrow();
        _loans.Add(loan);
        member.ActiveLoans.Add(loan);
        return loan;
    }

    // --- Return Rule: Calculate fine + notify reservation queue ---
    public decimal ReturnBook(Loan loan, Book book, DateTime returnDate)
    {
        if (loan.IsReturned) throw new InvalidOperationException("Book already returned.");
        
        loan.Return(returnDate);
        var fine = loan.CalculateFine(returnDate);
        
        CheckAndNotifyReservations(book);
        return fine;
    }

    private void CheckAndNotifyReservations(Book book)
    {
        // FIFO queue: oldest waiting reservation gets notified first
        var nextInQueue = _reservations
            .Where(r => r.Book == book && r.Status == ReservationStatus.Waiting)
            .OrderBy(r => r.ReservationDate)
            .FirstOrDefault();

        if (nextInQueue != null)
        {
            nextInQueue.MarkAsReady();
            Console.WriteLine($"🔔 [NOTIFICATION] '{book.Title}' is ready for pickup by {nextInQueue.Member.Name}!");
        }
    }

    // --- Reserve Rule: Max 5 simultaneous ---
    public Reservation ReserveBook(Member member, Book book)
    {
        int activeReservations = _reservations.Count(r => r.Book == book && r.IsActive());
        if (activeReservations >= 5)
            throw new InvalidOperationException("Reservation queue full (max 5).");

        var reservation = new Reservation(_nextReservationId++, book, member);
        _reservations.Add(reservation);
        return reservation;
    }
}