namespace AtmMachine;

public sealed class BankAccount
{
    private DateTime lastWithdrawalDate = DateTime.MinValue;

    public BankAccount(string accountNumber, decimal balance, decimal dailyWithdrawalLimit)
    {
        AccountNumber = !string.IsNullOrWhiteSpace(accountNumber) ? accountNumber : throw new ArgumentException("Account number is required.", nameof(accountNumber));
        Balance = balance >= 0 ? balance : throw new ArgumentOutOfRangeException(nameof(balance));
        DailyWithdrawalLimit = dailyWithdrawalLimit > 0 ? dailyWithdrawalLimit : throw new ArgumentOutOfRangeException(nameof(dailyWithdrawalLimit));
    }

    public string AccountNumber { get; }

    public decimal Balance { get; private set; }

    public decimal DailyWithdrawalLimit { get; }

    public decimal WithdrawnToday { get; private set; }

    public void Credit(decimal amount)
    {
        if (amount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount));
        }

        Balance += amount;
    }

    public void Debit(decimal amount)
    {
        if (amount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount));
        }

        if (Balance < amount)
        {
            throw new InvalidOperationException("Insufficient balance.");
        }

        Balance -= amount;
    }

    public void Withdraw(decimal amount, DateTime today)
    {
        ResetIfNewDay(today);

        if (amount > DailyWithdrawalLimit - WithdrawnToday)
        {
            throw new InvalidOperationException("Daily withdrawal limit exceeded.");
        }

        Debit(amount);
        WithdrawnToday += amount;
        lastWithdrawalDate = today.Date;
    }

    public void TransferTo(BankAccount target, decimal amount, DateTime today)
    {
        if (target is null)
        {
            throw new ArgumentNullException(nameof(target));
        }

        Withdraw(amount, today);
        target.Credit(amount);
    }

    private void ResetIfNewDay(DateTime today)
    {
        if (lastWithdrawalDate != today.Date)
        {
            WithdrawnToday = 0m;
            lastWithdrawalDate = today.Date;
        }
    }
}

public sealed class Card
{
    public Card(string cardNumber, string pin, BankAccount account)
    {
        CardNumber = !string.IsNullOrWhiteSpace(cardNumber) ? cardNumber : throw new ArgumentException("Card number is required.", nameof(cardNumber));
        Pin = !string.IsNullOrWhiteSpace(pin) ? pin : throw new ArgumentException("PIN is required.", nameof(pin));
        Account = account ?? throw new ArgumentNullException(nameof(account));
    }

    public string CardNumber { get; }

    public string Pin { get; }

    public BankAccount Account { get; }

    public bool IsLocked { get; private set; }

    public void Lock() => IsLocked = true;
}

public sealed class AtmSession(Card card)
{
    public Card Card { get; } = card ?? throw new ArgumentNullException(nameof(card));

    public int FailedPinAttempts { get; private set; }

    public bool IsAuthenticated { get; private set; }

    public bool IsActive { get; private set; } = true;

    public void RegisterPinAttempt(bool success)
    {
        if (!IsActive)
        {
            throw new InvalidOperationException("Session is closed.");
        }

        if (success)
        {
            IsAuthenticated = true;
            return;
        }

        FailedPinAttempts++;
        if (FailedPinAttempts >= 3)
        {
            Card.Lock();
            IsActive = false;
            throw new InvalidOperationException("Card locked after 3 failed PIN attempts.");
        }
    }

    public void End() => IsActive = false;
}

public sealed class CashInventory
{
    private readonly SortedDictionary<int, int> notes;

    public CashInventory(IDictionary<int, int> initialNotes)
    {
        notes = new SortedDictionary<int, int>(Comparer<int>.Create((left, right) => right.CompareTo(left)));
        foreach (var pair in initialNotes)
        {
            notes[pair.Key] = pair.Value;
        }
    }

    public bool CanDispense(decimal amount) => TryBuildDispensePlan(amount, out _);

    public IReadOnlyDictionary<int, int> Dispense(decimal amount)
    {
        if (!TryBuildDispensePlan(amount, out var plan))
        {
            throw new InvalidOperationException("ATM cannot dispense the exact amount.");
        }

        foreach (var pair in plan)
        {
            notes[pair.Key] -= pair.Value;
        }

        return plan;
    }

    public void AddCash(IDictionary<int, int> depositedNotes)
    {
        foreach (var pair in depositedNotes)
        {
            notes[pair.Key] = notes.TryGetValue(pair.Key, out var count) ? count + pair.Value : pair.Value;
        }
    }

    private bool TryBuildDispensePlan(decimal amount, out Dictionary<int, int> plan)
    {
        if (amount <= 0 || amount % 1 != 0)
        {
            plan = new Dictionary<int, int>();
            return false;
        }

        var target = (int)amount;
        var denoms = notes.Keys.ToArray();
        var result = new Dictionary<int, int>();

        bool Search(int index, int remaining)
        {
            if (remaining == 0)
            {
                return true;
            }

            if (index >= denoms.Length)
            {
                return false;
            }

            var denom = denoms[index];
            var available = notes[denom];
            var maxCount = Math.Min(available, remaining / denom);

            for (var count = maxCount; count >= 0; count--)
            {
                if (count > 0)
                {
                    result[denom] = count;
                }
                else
                {
                    result.Remove(denom);
                }

                if (Search(index + 1, remaining - (count * denom)))
                {
                    return true;
                }
            }

            result.Remove(denom);
            return false;
        }

        if (!Search(0, target))
        {
            plan = new Dictionary<int, int>();
            return false;
        }

        plan = new Dictionary<int, int>(result);
        return true;
    }
}

public sealed class ATM
{
    private readonly CashInventory cashInventory;
    private AtmSession? session;

    public ATM(CashInventory cashInventory)
    {
        this.cashInventory = cashInventory ?? throw new ArgumentNullException(nameof(cashInventory));
    }

    public void InsertCard(Card card)
    {
        if (card.IsLocked)
        {
            throw new InvalidOperationException("Card is locked.");
        }

        session = new AtmSession(card);
    }

    public void EnterPin(string pin)
    {
        EnsureSession();
        session!.RegisterPinAttempt(string.Equals(session.Card.Pin, pin, StringComparison.Ordinal));
    }

    public decimal CheckBalance()
    {
        EnsureAuthenticated();
        return session!.Card.Account.Balance;
    }

    public IReadOnlyDictionary<int, int> Withdraw(decimal amount)
    {
        EnsureAuthenticated();

        var account = session!.Card.Account;
        account.Withdraw(amount, DateTime.Today);

        if (!cashInventory.CanDispense(amount))
        {
            account.Credit(amount);
            throw new InvalidOperationException("ATM cannot dispense the exact amount.");
        }

        return cashInventory.Dispense(amount);
    }

    public void Deposit(decimal amount)
    {
        EnsureAuthenticated();
        session!.Card.Account.Credit(amount);
        cashInventory.AddCash(new Dictionary<int, int> { [(int)amount] = 1 });
    }

    public void Transfer(decimal amount, BankAccount target)
    {
        EnsureAuthenticated();
        session!.Card.Account.TransferTo(target, amount, DateTime.Today);
    }

    private void EnsureSession()
    {
        if (session is null || !session.IsActive)
        {
            throw new InvalidOperationException("Insert a valid card first.");
        }
    }

    private void EnsureAuthenticated()
    {
        EnsureSession();
        if (!session!.IsAuthenticated)
        {
            throw new InvalidOperationException("Enter a valid PIN first.");
        }
    }
}