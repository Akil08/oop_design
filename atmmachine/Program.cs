using AtmMachine;

var mainAccount = new BankAccount("ACC-1001", 5000m, 1000m);
var savingsAccount = new BankAccount("ACC-2002", 1500m, 1000m);
var card = new Card("CARD-1111", "1234", mainAccount);

var atm = new ATM(new CashInventory(new Dictionary<int, int>
{
    [1000] = 2,
    [500] = 4,
    [200] = 5,
    [100] = 10,
    [50] = 10
}));

try
{
    atm.InsertCard(card);
    atm.EnterPin("0000");
    atm.EnterPin("1111");
    atm.EnterPin("2222");
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"Session closed: {ex.Message}");
}

atm.InsertCard(card);
atm.EnterPin("1234");

Console.WriteLine($"Balance: {atm.CheckBalance():C}");
Console.WriteLine($"Withdraw 1200: {atm.Withdraw(1200m)}");

atm.Deposit(300m);
Console.WriteLine($"Balance after deposit: {atm.CheckBalance():C}");

atm.Transfer(400m, savingsAccount);
Console.WriteLine($"Main account: {mainAccount.Balance:C}");
Console.WriteLine($"Savings account: {savingsAccount.Balance:C}");