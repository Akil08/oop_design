namespace LibraryManagement.Models;

public class Member
{
    public int Id { get; init; }
    public string Name { get; init; }
    public List<Loan> ActiveLoans { get; private set; } = new();
    public List<Reservation> Reservations { get; private set; } = new();

    public Member(int id, string name)
    {
        Id = id;
        Name = name;
    }

    // Rule: Max 3 books at a time
    public bool CanBorrowMore() => ActiveLoans.Count(l => !l.IsReturned) < 3;

    public decimal GetTotalFines(DateTime currentDate, decimal dailyFineRate = 1.0m)
    {
        decimal total = 0;
        foreach (var loan in ActiveLoans)
            if (!loan.IsReturned)
                total += loan.CalculateFine(currentDate, dailyFineRate);
        return total;
    }
}