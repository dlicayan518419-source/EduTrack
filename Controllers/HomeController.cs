using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using EduTrack.Data;
using Microsoft.EntityFrameworkCore;

namespace EduTrack.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var username = HttpContext.Session.GetString("Username");
            var role = HttpContext.Session.GetString("Role");

            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Login", "Account");
            }

            ViewBag.Username = username;
            ViewBag.Role = role;

            // Get actual counts from database
            ViewBag.StudentCount = _context.Students.Count();
            ViewBag.TeacherCount = _context.Staff.Count(s => s.StaffRole == "Teacher");
            ViewBag.EnrolledCount = _context.Enrollments.Count(e => e.Status == "Active");

            // Get total collections (sum of AmountPaid from Payments)
            var totalCollections = _context.Payments.Sum(p => (decimal?)p.AmountPaid) ?? 0;
            ViewBag.TotalCollections = totalCollections.ToString("N2");

            return View();
        }
    }
}