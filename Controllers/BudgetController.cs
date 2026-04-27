using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using EduTrack.Data;
using EduTrack.Models;

namespace EduTrack.Controllers
{
    public class BudgetController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BudgetController(ApplicationDbContext context)
        {
            _context = context;
        }

        private bool IsAuthorized()
        {
            var role = HttpContext.Session.GetString("Role");
            return role == "Admin" || role == "Principal" || role == "Accounting";
        }

        // GET: Budget/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            if (!IsAuthorized()) return RedirectToAction("Login", "Account");

            try
            {
                // Student Payments Income (from Payment table)
                var studentPayments = await _context.Payments.SumAsync(p => (decimal?)p.AmountPaid) ?? 0;

                // External Income (from FinancialTransaction table)
                var externalIncome = await _context.FinancialTransactions
                    .Where(t => t.Type == "Income")
                    .SumAsync(t => (decimal?)t.Amount) ?? 0;

                // Total Income (Student Payments + External Income)
                var totalIncome = studentPayments + externalIncome;

                // Total Expenses (from FinancialTransaction table)
                var totalExpenses = await _context.FinancialTransactions
                    .Where(t => t.Type == "Expense")
                    .SumAsync(t => (decimal?)t.Amount) ?? 0;

                // Total Budget = Total Income
                var totalBudget = totalIncome;

                var budgetUtilization = totalBudget > 0 ? (totalExpenses / totalBudget) * 100 : 0;

                ViewBag.TotalBudget = totalBudget.ToString("N2");
                ViewBag.TotalExpenses = totalExpenses.ToString("N2");
                ViewBag.TotalIncome = totalIncome.ToString("N2");
                ViewBag.StudentPayments = studentPayments.ToString("N2");
                ViewBag.ExternalIncome = externalIncome.ToString("N2");
                ViewBag.BudgetUtilization = Math.Round(budgetUtilization, 1);
                ViewBag.RemainingBudget = (totalBudget - totalExpenses).ToString("N2");

                return View();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return View();
            }
        }

        // GET: Budget/Expenses
        public async Task<IActionResult> Expenses()
        {
            if (!IsAuthorized()) return RedirectToAction("Login", "Account");

            var expenses = await _context.FinancialTransactions
                .Where(t => t.Type == "Expense")
                .OrderByDescending(t => t.Date)
                .ToListAsync();

            return View(expenses);
        }

        // GET: Budget/CreateExpense
        public IActionResult CreateExpense()
        {
            if (!IsAuthorized()) return RedirectToAction("Login", "Account");
            return View();
        }

        // POST: Budget/CreateExpense
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateExpense(FinancialTransaction transaction)
        {
            if (!IsAuthorized()) return RedirectToAction("Login", "Account");

            if (ModelState.IsValid)
            {
                transaction.Type = "Expense";
                transaction.Date = DateTime.Now;
                _context.Add(transaction);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Expenses));
            }
            return View(transaction);
        }

        // GET: Budget/Income
        public async Task<IActionResult> Income()
        {
            if (!IsAuthorized()) return RedirectToAction("Login", "Account");

            // Get student payments as income records
            var studentPayments = await _context.Payments
                .Include(p => p.Billing)
                .ThenInclude(b => b.Student)
                .Select(p => new FinancialTransaction
                {
                    Date = p.PaymentDate,
                    Type = "Income",
                    Category = "Student Payment",
                    Amount = p.AmountPaid,
                    Description = $"Payment from {p.Billing.Student.FirstName} {p.Billing.Student.LastName} - Receipt: {p.ReceiptNumber}"
                })
                .ToListAsync();

            // Get external income
            var externalIncome = await _context.FinancialTransactions
                .Where(t => t.Type == "Income")
                .OrderByDescending(t => t.Date)
                .ToListAsync();

            // Combine both sources
            var allIncome = studentPayments.Cast<FinancialTransaction>()
                .Concat(externalIncome)
                .OrderByDescending(t => t.Date)
                .ToList();

            return View(allIncome);
        }

        // GET: Budget/CreateIncome (External Income only)
        public IActionResult CreateIncome()
        {
            if (!IsAuthorized()) return RedirectToAction("Login", "Account");
            return View();
        }

        // POST: Budget/CreateIncome (External Income only)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateIncome(FinancialTransaction transaction)
        {
            if (!IsAuthorized()) return RedirectToAction("Login", "Account");

            if (ModelState.IsValid)
            {
                transaction.Type = "Income";
                transaction.Date = DateTime.Now;
                _context.Add(transaction);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Income));
            }
            return View(transaction);
        }

        // GET: Budget/BudgetReport
        public async Task<IActionResult> BudgetReport()
        {
            if (!IsAuthorized()) return RedirectToAction("Login", "Account");

            // Get monthly student payments
            var studentPaymentsMonthly = await _context.Payments
                .GroupBy(p => new { p.PaymentDate.Year, p.PaymentDate.Month })
                .Select(g => new
                {
                    g.Key.Year,
                    g.Key.Month,
                    Total = g.Sum(p => p.AmountPaid)
                })
                .ToListAsync();

            // Get monthly external income
            var externalIncomeMonthly = await _context.FinancialTransactions
                .Where(t => t.Type == "Income")
                .GroupBy(t => new { t.Date.Year, t.Date.Month })
                .Select(g => new
                {
                    g.Key.Year,
                    g.Key.Month,
                    Total = g.Sum(t => t.Amount)
                })
                .ToListAsync();

            // Get monthly expenses
            var expensesRaw = await _context.FinancialTransactions
                .Where(t => t.Type == "Expense")
                .GroupBy(t => new { t.Date.Year, t.Date.Month })
                .Select(g => new
                {
                    g.Key.Year,
                    g.Key.Month,
                    Total = g.Sum(t => t.Amount)
                })
                .ToListAsync();

            // Combine student payments and external income for monthly income
            var allIncomeMonthly = studentPaymentsMonthly
                .Concat(externalIncomeMonthly)
                .GroupBy(x => new { x.Year, x.Month })
                .Select(g => new
                {
                    g.Key.Year,
                    g.Key.Month,
                    Total = g.Sum(x => x.Total)
                })
                .OrderByDescending(x => x.Year)
                .ThenByDescending(x => x.Month)
                .ToList();

            // Convert to view models with month names
            var monthlyIncome = allIncomeMonthly.Select(x => new
            {
                x.Year,
                x.Month,
                MonthName = GetMonthNameStatic(x.Month),
                x.Total
            }).ToList();

            var monthlyExpenses = expensesRaw.Select(x => new
            {
                x.Year,
                x.Month,
                MonthName = GetMonthNameStatic(x.Month),
                x.Total
            }).ToList();

            var byCategory = await _context.FinancialTransactions
                .Where(t => t.Type == "Expense")
                .GroupBy(t => t.Category)
                .Select(g => new
                {
                    Category = g.Key ?? "Uncategorized",
                    Total = g.Sum(t => t.Amount)
                })
                .OrderByDescending(x => x.Total)
                .ToListAsync();

            // Total summary
            var totalStudentPayments = await _context.Payments.SumAsync(p => (decimal?)p.AmountPaid) ?? 0;
            var totalExternalIncome = await _context.FinancialTransactions
                .Where(t => t.Type == "Income")
                .SumAsync(t => (decimal?)t.Amount) ?? 0;
            var totalExpenses = await _context.FinancialTransactions
                .Where(t => t.Type == "Expense")
                .SumAsync(t => (decimal?)t.Amount) ?? 0;

            ViewBag.TotalStudentPayments = totalStudentPayments.ToString("N2");
            ViewBag.TotalExternalIncome = totalExternalIncome.ToString("N2");
            ViewBag.TotalIncome = (totalStudentPayments + totalExternalIncome).ToString("N2");
            ViewBag.TotalExpenses = totalExpenses.ToString("N2");
            ViewBag.NetSurplus = (totalStudentPayments + totalExternalIncome - totalExpenses).ToString("N2");

            ViewBag.MonthlyExpenses = monthlyExpenses;
            ViewBag.MonthlyIncome = monthlyIncome;
            ViewBag.ByCategory = byCategory;

            return View();
        }

        private static string GetMonthNameStatic(int month)
        {
            return System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month);
        }
    }
}