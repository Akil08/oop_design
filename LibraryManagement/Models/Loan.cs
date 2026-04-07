namespace LibraryManagement.Models;

public class Loan
{
    public int LoanId { get; init; }
    public BookCopy BookCopy { get; init; }
    public Member Member { get; init; }
    public DateTime BorrowDate { get; init; }
    public DateTime DueDate { get; init; }
    public DateTime? ReturnDate { get; private set; }
    public bool IsReturned => ReturnDate.HasValue;

    public Loan(int loanId, BookCopy copy, Member member)
    {
        LoanId = loanId;
        BookCopy = copy;
        Member = member;
        BorrowDate = DateTime.Today;
        DueDate = BorrowDate.AddDays(14); // Rule: 14-day limit
    }

    public void Return(DateTime returnDate)
    {
        if (IsReturned) throw new InvalidOperationException("This book has already been returned.");
        if (returnDate < BorrowDate) throw new ArgumentException("Return date cannot be before borrow date.");
        
        ReturnDate = returnDate;
        BookCopy.Return(); // Updates copy status back to Available
    }

    public int DaysOverdue(DateTime currentDate)
    {
        DateTime checkDate = IsReturned ? ReturnDate!.Value : currentDate;
        return checkDate > DueDate ? (checkDate - DueDate).Days : 0;
    }

    public decimal CalculateFine(DateTime currentDate, decimal dailyRate = 1.0m)
    {
        int overdue = DaysOverdue(currentDate);
        return overdue <= 0 ? 0 : overdue * dailyRate;
    }
}