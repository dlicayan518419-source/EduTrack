using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using EduTrack.Data;
using EduTrack.Models;

namespace EduTrack.Controllers
{
    public class HealthController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HealthController(ApplicationDbContext context)
        {
            _context = context;
        }

        private bool IsAuthorized()
        {
            var role = HttpContext.Session.GetString("Role");
            return role == "Admin" || role == "Principal" || role == "Nurse";
        }

        // GET: Health
        public async Task<IActionResult> Index()
        {
            if (!IsAuthorized()) return RedirectToAction("Login", "Account");

            var healthRecords = await _context.HealthRecords
                .Include(h => h.Student)
                .OrderByDescending(h => h.Date)
                .ToListAsync();

            return View(healthRecords);
        }

        // GET: Health/Create
        public async Task<IActionResult> Create()
        {
            if (!IsAuthorized()) return RedirectToAction("Login", "Account");

            ViewBag.Students = await _context.Students.Where(s => s.Status == "Enrolled").ToListAsync();
            return View();
        }

        // POST: Health/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(HealthRecord healthRecord)
        {
            if (!IsAuthorized()) return RedirectToAction("Login", "Account");

            if (ModelState.IsValid)
            {
                healthRecord.Date = DateTime.Now;
                _context.Add(healthRecord);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Students = await _context.Students.Where(s => s.Status == "Enrolled").ToListAsync();
            return View(healthRecord);
        }
    }
}