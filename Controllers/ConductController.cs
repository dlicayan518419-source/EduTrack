using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using EduTrack.Data;
using EduTrack.Models;

namespace EduTrack.Controllers
{
    public class ConductController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ConductController(ApplicationDbContext context)
        {
            _context = context;
        }

        private bool IsAuthorized()
        {
            var role = HttpContext.Session.GetString("Role");
            return role == "Admin" || role == "Principal" || role == "Teacher" || role == "Guidance";
        }

        // GET: Conduct
        public async Task<IActionResult> Index()
        {
            if (!IsAuthorized()) return RedirectToAction("Login", "Account");

            try
            {
                var conductRecords = await _context.ConductRecords
                    .Include(c => c.Student)
                    .Include(c => c.Recorder)
                    .OrderByDescending(c => c.Date)
                    .Select(c => new
                    {
                        c.ConductID,
                        c.Date,
                        c.Type,
                        c.Description,
                        c.ActionTaken,
                        StudentName = c.Student != null ? (c.Student.FirstName ?? "") + " " + (c.Student.LastName ?? "") : "Unknown",
                        RecorderName = c.Recorder != null ? (c.Recorder.FirstName ?? "") + " " + (c.Recorder.LastName ?? "") : "Unknown",
                        RecorderRole = c.Recorder != null ? (c.Recorder.StaffRole ?? "") : ""
                    })
                    .ToListAsync();

                return View(conductRecords);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading conduct records: {ex.Message}");
                return View(new List<dynamic>());
            }
        }

        // GET: Conduct/Create
        public async Task<IActionResult> Create()
        {
            if (!IsAuthorized()) return RedirectToAction("Login", "Account");

            try
            {
                var students = await _context.Students
                    .Where(s => s.Status == "Enrolled")
                    .Select(s => new { s.StudentID, FullName = (s.FirstName ?? "") + " " + (s.LastName ?? "") })
                    .ToListAsync();

                var staff = await _context.Staff
                    .Where(s => s.StaffRole == "Teacher" || s.StaffRole == "Guidance")
                    .Select(s => new { s.StaffID, FullName = (s.FirstName ?? "") + " " + (s.LastName ?? ""), s.StaffRole })
                    .ToListAsync();

                ViewBag.Students = students;
                ViewBag.Staff = staff;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading data: {ex.Message}");
                ViewBag.Students = new List<dynamic>();
                ViewBag.Staff = new List<dynamic>();
            }

            return View();
        }

        // POST: Conduct/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ConductRecord conductRecord)
        {
            if (!IsAuthorized()) return RedirectToAction("Login", "Account");

            if (ModelState.IsValid)
            {
                try
                {
                    conductRecord.Date = DateTime.Now;
                    conductRecord.Type = conductRecord.Type ?? "Positive";
                    conductRecord.Description = conductRecord.Description ?? "";
                    conductRecord.ActionTaken = conductRecord.ActionTaken ?? "";

                    _context.Add(conductRecord);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error saving conduct record: {ex.Message}");
                    ModelState.AddModelError("", "Error saving record. Please try again.");
                }
            }

            // Reload data if validation fails
            var students = await _context.Students
                .Where(s => s.Status == "Enrolled")
                .Select(s => new { s.StudentID, FullName = (s.FirstName ?? "") + " " + (s.LastName ?? "") })
                .ToListAsync();

            var staff = await _context.Staff
                .Where(s => s.StaffRole == "Teacher" || s.StaffRole == "Guidance")
                .Select(s => new { s.StaffID, FullName = (s.FirstName ?? "") + " " + (s.LastName ?? ""), s.StaffRole })
                .ToListAsync();

            ViewBag.Students = students;
            ViewBag.Staff = staff;

            return View(conductRecord);
        }

        // GET: Conduct/Details/5
        public async Task<IActionResult> Details(int id)
        {
            if (!IsAuthorized()) return RedirectToAction("Login", "Account");

            try
            {
                var conductRecord = await _context.ConductRecords
                    .Include(c => c.Student)
                    .Include(c => c.Recorder)
                    .Select(c => new
                    {
                        c.ConductID,
                        c.Date,
                        c.Type,
                        c.Description,
                        c.ActionTaken,
                        StudentId = c.Student != null ? c.Student.StudentID : 0,
                        StudentName = c.Student != null ? (c.Student.FirstName ?? "") + " " + (c.Student.LastName ?? "") : "Unknown",
                        RecorderName = c.Recorder != null ? (c.Recorder.FirstName ?? "") + " " + (c.Recorder.LastName ?? "") : "Unknown",
                        RecorderRole = c.Recorder != null ? (c.Recorder.StaffRole ?? "") : ""
                    })
                    .FirstOrDefaultAsync(c => c.ConductID == id);

                if (conductRecord == null) return NotFound();

                return View(conductRecord);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading details: {ex.Message}");
                return NotFound();
            }
        }
    }
}