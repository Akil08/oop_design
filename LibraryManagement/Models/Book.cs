using System.Linq;

namespace LibraryManagement.Models;

public class Book
{
    public string Isbn { get; init; }
    public string Title { get; init; }
    public string Author { get; init; }
    public List<BookCopy> Copies { get; private set; } = new();

    public Book(string isbn, string title, string author, int initialCopies)
    {
        Isbn = isbn;
        Title = title;
        Author = author;

        for (int i = 1; i <= initialCopies; i++)
            Copies.Add(new BookCopy(i));
    }

    public BookCopy? GetAvailableCopy() => Copies.FirstOrDefault(c => c.IsAvailable());
    public int AvailableCount() => Copies.Count(c => c.IsAvailable());
}