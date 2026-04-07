using System;
using System.Linq;
using LibraryManagement.Services;
using LibraryManagement.Models;

namespace LibraryManagement;

public class Program
{
    
    private static readonly LibraryManager _manager =  LibraryManager.GetInstance();
    public static void Main(string[] args)
    {
        Console.WriteLine("📚 Welcome to Library Management System!");
        bool running = true;

        while (running)
        {
            ShowMenu();
            string? input = Console.ReadLine()?.Trim();

            try
            {
                switch (input)
                {
                    case "1": HandleAddBook(); break;
                    case "2": HandleAddMember(); break;
                    case "3": HandleBorrow(); break;
                    case "4": HandleReturn(); break;
                    case "5": HandleReserve(); break;
                    case "6": HandleShowFines(); break;
                    case "0": running = false; Console.WriteLine("👋 Goodbye!"); break;
                    default: Console.WriteLine("⚠️ Invalid option. Try again."); break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error: {ex.Message}");
            }

            if (running)
            {
                Console.WriteLine("\nPress Enter to continue...");
                Console.ReadLine();
                Console.Clear();
            }
        }
    }

    private static void ShowMenu()
    {
        Console.WriteLine("\n=== MAIN MENU ===");
        Console.WriteLine("1. Add Book");
        Console.WriteLine("2. Add Member");
        Console.WriteLine("3. Borrow Book");
        Console.WriteLine("4. Return Book");
        Console.WriteLine("5. Reserve Book");
        Console.WriteLine("6. Check Member Fines & Loans");
        Console.WriteLine("0. Exit");
        Console.Write("Select option: ");
    }

    private static void HandleAddBook()
    {
        Console.Write("ISBN: ");
        string isbn = Console.ReadLine()!.Trim();
        Console.Write("Title: ");
        string title = Console.ReadLine()!.Trim();
        Console.Write("Author: ");
        string author = Console.ReadLine()!.Trim();
        Console.Write("Copies: ");
        int copies = int.Parse(Console.ReadLine()!.Trim());

        var book = _manager.AddBook(isbn, title, author, copies);
        Console.WriteLine($"✅ Added '{book.Title}' ({copies} copies)");
    }

    private static void HandleAddMember()
    {
        Console.Write("Member Name: ");
        string name = Console.ReadLine()!.Trim();
        var member = _manager.AddMember(name);
        Console.WriteLine($"✅ Added: {member.Name} (ID: {member.Id})");
    }

    private static void HandleBorrow()
    {
        Console.Write("Member ID: ");
        int memberId = int.Parse(Console.ReadLine()!.Trim());
        var member = _manager.FindMemberById(memberId) ?? throw new Exception("Member not found.");

        Console.Write("Book ISBN: ");
        string isbn = Console.ReadLine()!.Trim();
        var book = _manager.FindBookByIsbn(isbn) ?? throw new Exception("Book not found.");

        var loan = _manager.BorrowBook(member, book);
        Console.WriteLine($"✅ Borrowed '{book.Title}'. Due: {loan.DueDate:d}");
    }

    private static void HandleReturn()
    {
        Console.Write("Member ID: ");
        int memberId = int.Parse(Console.ReadLine()!.Trim());
        var member = _manager.FindMemberById(memberId) ?? throw new Exception("Member not found.");

        Console.Write("Book ISBN: ");
        string isbn = Console.ReadLine()!.Trim();
        var book = _manager.FindBookByIsbn(isbn) ?? throw new Exception("Book not found.");

        // Find the active loan for this exact book copy
        var loan = member.ActiveLoans.FirstOrDefault(l => !l.IsReturned && book.Copies.Contains(l.BookCopy))
                   ?? throw new Exception("No active loan found for this book.");

        Console.Write("Return date (YYYY-MM-DD) or press Enter for today: ");
        string dateInput = Console.ReadLine()!.Trim();
        DateTime returnDate = string.IsNullOrEmpty(dateInput) ? DateTime.Today : DateTime.Parse(dateInput);

        decimal fine = _manager.ReturnBook(loan, book, returnDate);
        Console.WriteLine($"✅ Returned. Fine charged: ${fine:F2}");
    }

    private static void HandleReserve()
    {
        Console.Write("Member ID: ");
        int memberId = int.Parse(Console.ReadLine()!.Trim());
        var member = _manager.FindMemberById(memberId) ?? throw new Exception("Member not found.");

        Console.Write("Book ISBN: ");
        string isbn = Console.ReadLine()!.Trim();
        var book = _manager.FindBookByIsbn(isbn) ?? throw new Exception("Book not found.");

        var res = _manager.ReserveBook(member, book);
        Console.WriteLine($"✅ Reserved '{book.Title}'. You'll be notified when available.");
    }

    private static void HandleShowFines()
    {
        Console.Write("Member ID: ");
        int memberId = int.Parse(Console.ReadLine()!.Trim());
        var member = _manager.FindMemberById(memberId) ?? throw new Exception("Member not found.");

        decimal totalFines = member.GetTotalFines(DateTime.Today);
        Console.WriteLine($"\n👤 {member.Name} | Total Fines: ${totalFines:F2}");
        Console.WriteLine("📖 Active Loans:");
        foreach (var loan in member.ActiveLoans.Where(l => !l.IsReturned))
        {
            Console.WriteLine($"   - ISBN: {loan.BookCopy.CopyId} | Due: {loan.DueDate:d} | Overdue: {loan.DaysOverdue(DateTime.Today)} days");
        }
    }
}