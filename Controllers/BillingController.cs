using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using EduTrack.Data;
using EduTrack.Models;

namespace EduTrack.Controllers
{
    public class BillingController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BillingController(ApplicationDbContext context)
        {
            _context = context;
        }

        private bool IsAuthorized()
        {
            var role = HttpContext.Session.GetString("Role");
            return role == "Admin" || role == "Principal" || role == "Cashier" || role == "Accounting";
        }

        // GET: Billing
        public async Task<IActionResult> Index()
        {
            if (!IsAuthorized()) return RedirectToAction("Login", "Account");

            try
            {
                var billings = await _context.Billings
                    .Include(b => b.Student)
                    .Include(b => b.Payments)
                    .Select(b => new
                    {
                        b.BillingID,
                        StudentName = b.Student != null ? (b.Student.FirstName ?? "") + " " + (b.Student.LastName ?? "") : "Unknown",
                        b.SchoolYear,
                        b.FeeType,
                        b.Amount,
                        b.DueDate,
                        b.Balance,
                        TotalPaid = b.Payments != null ? b.Payments.Sum(p => p.AmountPaid) : 0
                    })
                    .ToListAsync();

                return View(billings);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading billings: {ex.Message}");
                return View(new List<dynamic>());
            }
        }

        // GET: Billing/Create
        public async Task<IActionResult> Create()
        {
            if (!IsAuthorized()) return RedirectToAction("Login", "Account");

            var students = await _context.Students
                .Where(s => s.Status == "Enrolled")
                .Select(s => new { s.StudentID, FullName = (s.FirstName ?? "") + " " + (s.LastName ?? "") })
                .ToListAsync();

            ViewBag.Students = students;
            return View();
        }

        // POST: Billing/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Billing billing)
        {
            if (!IsAuthorized()) return RedirectToAction("Login", "Account");

            if (ModelState.IsValid)
            {
                billing.Balance = billing.Amount;
                _context.Add(billing);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            var students = await _context.Students
                .Where(s => s.Status == "Enrolled")
                .Select(s => new { s.StudentID, FullName = (s.FirstName ?? "") + " " + (s.LastName ?? "") })
                .ToListAsync();

            ViewBag.Students = students;
            return View(billing);
        }

        // GET: Billing/MakePayment/5
        public async Task<IActionResult> MakePayment(int id)
        {
            if (!IsAuthorized()) return RedirectToAction("Login", "Account");

            var billing = await _context.Billings
                .Include(b => b.Student)
                .FirstOrDefaultAsync(b => b.BillingID == id);

            if (billing == null) return NotFound();

            ViewBag.Billing = billing;
            ViewBag.StudentName = billing.Student != null ? (billing.Student.FirstName ?? "") + " " + (billing.Student.LastName ?? "") : "Unknown";

            return View();
        }

        // POST: Billing/RecordPayment
        [HttpPost]
        public async Task<IActionResult> RecordPayment(int billingId, decimal amountPaid, string referenceNumber)
        {
            if (!IsAuthorized()) return RedirectToAction("Login", "Account");

            try
            {
                var billing = await _context.Billings.FindAsync(billingId);
                if (billing == null) return NotFound();

                var payment = new Payment
                {
                    BillingID = billingId,
                    PaymentDate = DateTime.Now,
                    AmountPaid = amountPaid,
                    ReferenceNumber = referenceNumber ?? "",
                    ReceiptNumber = "REC-" + DateTime.Now.Ticks.ToString()
                };

                _context.Payments.Add(payment);

                billing.Balance -= amountPaid;
                _context.Update(billing);

                await _context.SaveChangesAsync();

                TempData["Success"] = "Payment recorded successfully!";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error recording payment: {ex.Message}");
                TempData["Error"] = "Error recording payment.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}