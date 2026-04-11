namespace LibraryManagement.Models;

public enum CopyStatus { Available, Borrowed, OnHold }

public class BookCopy
{
    public int CopyId { get; init; }
    public CopyStatus Status { get; private set; } = CopyStatus.Available;

    public BookCopy(int copyId) => CopyId = copyId;

    public bool IsAvailable() => Status == CopyStatus.Available;
    public void Borrow() => Status = CopyStatus.Borrowed;
    public void Return() => Status = CopyStatus.Available;
    public void PutOnHold() => Status = CopyStatus.OnHold;
}

