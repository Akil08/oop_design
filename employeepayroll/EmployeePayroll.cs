namespace EmployeePayroll;

public sealed class Department(string name)
{
    private readonly List<Employee> employees = new();

    public string Name { get; } = !string.IsNullOrWhiteSpace(name) ? name : throw new ArgumentException("Department name is required.", nameof(name));

    public IReadOnlyCollection<Employee> Employees => employees.AsReadOnly();

    public void AddEmployee(Employee employee)
    {
        if (employee is null)
        {
            throw new ArgumentNullException(nameof(employee));
        }

        employees.Add(employee);
    }
}

public abstract class Employee(string name, string departmentName)
{
    public string Name { get; } = !string.IsNullOrWhiteSpace(name) ? name : throw new ArgumentException("Employee name is required.", nameof(name));

    public string DepartmentName { get; } = !string.IsNullOrWhiteSpace(departmentName) ? departmentName : throw new ArgumentException("Department name is required.", nameof(departmentName));

    public abstract decimal GrossPay { get; }

    public abstract decimal TaxRate { get; }

    public decimal Deductions => GrossPay * TaxRate;

    public decimal NetPay => GrossPay - Deductions;
}

public sealed class FullTimeEmployee(string name, string departmentName, decimal monthlySalary, decimal bonus) : Employee(name, departmentName)
{
    public override decimal GrossPay => monthlySalary + bonus;

    public override decimal TaxRate => 0.12m;
}

public sealed class PartTimeEmployee(string name, string departmentName, decimal hourlyRate, decimal hoursWorked) : Employee(name, departmentName)
{
    public override decimal GrossPay => hourlyRate * hoursWorked;

    public override decimal TaxRate => 0.08m;
}

public sealed class Contractor(string name, string departmentName, int projectCount, decimal amountPerProject) : Employee(name, departmentName)
{
    public override decimal GrossPay => projectCount * amountPerProject;

    public override decimal TaxRate => 0.18m;
}

public sealed record PayrollLine(string EmployeeName, decimal GrossPay, decimal Deductions, decimal NetPay);

public sealed class PayrollReport(string departmentName, IReadOnlyCollection<PayrollLine> lines)
{
    public string DepartmentName { get; } = departmentName;

    public IReadOnlyCollection<PayrollLine> Lines { get; } = lines;
}

public static class PayrollService
{
    public static PayrollReport GenerateMonthlyReport(Department department)
    {
        if (department is null)
        {
            throw new ArgumentNullException(nameof(department));
        }

        var lines = department.Employees
            .Select(employee => new PayrollLine(employee.Name, employee.GrossPay, employee.Deductions, employee.NetPay))
            .ToList();

        return new PayrollReport(department.Name, lines);
    }
}