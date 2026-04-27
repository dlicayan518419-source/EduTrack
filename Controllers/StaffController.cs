using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using EduTrack.Data;
using EduTrack.Models;

namespace EduTrack.Controllers
{
    public class StaffController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StaffController(ApplicationDbContext context)
        {
            _context = context;
        }

        private bool IsAuthorized()
        {
            var role = HttpContext.Session.GetString("Role");
            return role == "Admin" || role == "Principal";
        }

        // GET: Staff
        public async Task<IActionResult> Index()
        {
            if (!IsAuthorized()) return RedirectToAction("Login", "Account");
            var staff = await _context.Staff.Include(s => s.User).ToListAsync();
            return View(staff);
        }

        // GET: Staff/Create
        public IActionResult Create()
        {
            if (!IsAuthorized()) return RedirectToAction("Login", "Account");
            return View();
        }

        // POST: Staff/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Staff staff)
        {
            if (!IsAuthorized()) return RedirectToAction("Login", "Account");
            if (ModelState.IsValid)
            {
                _context.Add(staff);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(staff);
        }

        // GET: Staff/Update/5
        public async Task<IActionResult> Update(int id)
        {
            if (!IsAuthorized()) return RedirectToAction("Login", "Account");
            var staff = await _context.Staff.FindAsync(id);
            if (staff == null) return NotFound();
            return View(staff);
        }

        // POST: Staff/Update/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int id, Staff staff)
        {
            if (!IsAuthorized()) return RedirectToAction("Login", "Account");
            if (id != staff.StaffID) return NotFound();
            if (ModelState.IsValid)
            {
                _context.Update(staff);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(staff);
        }
    }
}