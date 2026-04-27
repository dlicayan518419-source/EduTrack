using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using EduTrack.Data;
using EduTrack.Models;

namespace EduTrack.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Check if user is logged in and is Admin
        private bool IsAdmin()
        {
            var role = HttpContext.Session.GetString("Role");
            return role == "Admin";
        }

        // GET: Admin/Dashboard
        public IActionResult Dashboard()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            return View();
        }

        // GET: Admin/Users
        public async Task<IActionResult> Users()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            var users = await _context.Users.Include(u => u.Role).ToListAsync();
            return View(users);
        }

        // GET: Admin/Roles
        public async Task<IActionResult> Roles()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            var roles = await _context.Roles.ToListAsync();
            return View(roles);
        }

        // GET: Admin/SystemLogs
        public async Task<IActionResult> SystemLogs()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            var logs = await _context.SystemLogs.Include(l => l.User).OrderByDescending(l => l.Timestamp).Take(100).ToListAsync();
            return View(logs);
        }

        // GET: Admin/GradeLevels
        public async Task<IActionResult> GradeLevels()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            var gradeLevels = await _context.GradeLevels.ToListAsync();
            return View(gradeLevels);
        }

        // GET: Admin/Sections
        public async Task<IActionResult> Sections()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            var sections = await _context.Sections.Include(s => s.GradeLevel).Include(s => s.Adviser).ToListAsync();
            return View(sections);
        }

        // GET: Admin/Subjects
        public async Task<IActionResult> Subjects()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            var subjects = await _context.Subjects.Include(s => s.GradeLevel).ToListAsync();
            return View(subjects);
        }
    }
}