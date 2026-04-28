namespace HotelBooking.Domain;

public sealed class Guest
{
    public Guest(string name, string email)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Guest name is required.", nameof(name));
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("Guest email is required.", nameof(email));
        }

        Name = name;
        Email = email;
    }

    public string Name { get; }

    public string Email { get; }
}