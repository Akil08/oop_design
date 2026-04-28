using EmployeePayroll;

var department = new Department("Engineering");
department.AddEmployee(new FullTimeEmployee("Ananya", "Engineering", 70000m, 5000m));
department.AddEmployee(new PartTimeEmployee("Ravi", "Engineering", 120m, 80m));
department.AddEmployee(new Contractor("Meera", "Engineering", 3, 15000m));

var report = PayrollService.GenerateMonthlyReport(department);

Console.WriteLine($"Department: {report.DepartmentName}");
foreach (var line in report.Lines)
{
    Console.WriteLine($"{line.EmployeeName} | Gross: {line.GrossPay:C} | Deduction: {line.Deductions:C} | Net: {line.NetPay:C}");
}