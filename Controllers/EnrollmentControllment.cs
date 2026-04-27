using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using EduTrack.Data;
using EduTrack.Models;

namespace EduTrack.Controllers
{
    public class EnrollmentController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EnrollmentController(ApplicationDbContext context)
        {
            _context = context;
        }

        private bool IsAuthorized()
        {
            var role = HttpContext.Session.GetString("Role");
            return role == "Admin" || role == "Principal" || role == "Registrar";
        }

        // GET: Enrollment
        public async Task<IActionResult> Index()
        {
            if (!IsAuthorized()) return RedirectToAction("Login", "Account");

            try
            {
                var enrollments = await _context.Enrollments
                    .Include(e => e.Student)
                    .Include(e => e.Section)
                        .ThenInclude(s => s != null ? s.GradeLevel : null)
                    .Include(e => e.Classification)
                    .ToListAsync();

                return View(enrollments);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading enrollments: {ex.Message}");
                return View(new List<Enrollment>());
            }
        }

        // GET: Enrollment/Create
        public async Task<IActionResult> Create()
        {
            if (!IsAuthorized()) return RedirectToAction("Login", "Account");

            // Load data with null-safe projections
            var students = await _context.Students
                .Where(s => s.Status == "Enrolled")
                .Select(s => new { s.StudentID, FullName = (s.FirstName ?? "") + " " + (s.LastName ?? "") })
                .ToListAsync();

            var sections = await _context.Sections
                .Include(s => s.GradeLevel)
                .Select(s => new {
                    s.SectionID,
                    DisplayName = (s.GradeLevel != null ? (s.GradeLevel.GradeName ?? "") : "") + " - Section " + (s.SectionName ?? "")
                })
                .ToListAsync();

            var classifications = await _context.AcademicClassifications
                .Select(c => new { c.ClassificationID, c.ClassificationName })
                .ToListAsync();

            ViewBag.Students = students;
            ViewBag.Sections = sections;
            ViewBag.Classifications = classifications;

            return View();
        }

        // POST: Enrollment/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Enrollment enrollment)
        {
            if (!IsAuthorized()) return RedirectToAction("Login", "Account");

            if (ModelState.IsValid)
            {
                enrollment.EnrollmentDate = DateTime.Now;
                enrollment.Status = "Active";
                enrollment.SchoolYear = enrollment.SchoolYear ?? "2025-2026";

                _context.Add(enrollment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // Reload data if validation fails
            var students = await _context.Students
                .Where(s => s.Status == "Enrolled")
                .Select(s => new { s.StudentID, FullName = (s.FirstName ?? "") + " " + (s.LastName ?? "") })
                .ToListAsync();

            var sections = await _context.Sections
                .Include(s => s.GradeLevel)
                .Select(s => new {
                    s.SectionID,
                    DisplayName = (s.GradeLevel != null ? (s.GradeLevel.GradeName ?? "") : "") + " - Section " + (s.SectionName ?? "")
                })
                .ToListAsync();

            var classifications = await _context.AcademicClassifications
                .Select(c => new { c.ClassificationID, c.ClassificationName })
                .ToListAsync();

            ViewBag.Students = students;
            ViewBag.Sections = sections;
            ViewBag.Classifications = classifications;

            return View(enrollment);
        }

        // GET: Enrollment/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            if (!IsAuthorized()) return RedirectToAction("Login", "Account");

            var enrollment = await _context.Enrollments
                .Include(e => e.Student)
                .Include(e => e.Section)
                .FirstOrDefaultAsync(e => e.EnrollmentID == id);

            if (enrollment == null) return NotFound();

            var students = await _context.Students
                .Select(s => new { s.StudentID, FullName = (s.FirstName ?? "") + " " + (s.LastName ?? "") })
                .ToListAsync();

            var sections = await _context.Sections
                .Include(s => s.GradeLevel)
                .Select(s => new {
                    s.SectionID,
                    DisplayName = (s.GradeLevel != null ? (s.GradeLevel.GradeName ?? "") : "") + " - Section " + (s.SectionName ?? "")
                })
                .ToListAsync();

            var classifications = await _context.AcademicClassifications
                .Select(c => new { c.ClassificationID, c.ClassificationName })
                .ToListAsync();

            ViewBag.Students = students;
            ViewBag.Sections = sections;
            ViewBag.Classifications = classifications;
            ViewBag.CurrentEnrollment = enrollment;

            return View(enrollment);
        }

        // POST: Enrollment/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Enrollment enrollment)
        {
            if (!IsAuthorized()) return RedirectToAction("Login", "Account");
            if (id != enrollment.EnrollmentID) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    enrollment.SchoolYear = enrollment.SchoolYear ?? "2025-2026";
                    _context.Update(enrollment);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Enrollments.Any(e => e.EnrollmentID == id))
                        return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }

            var students = await _context.Students
                .Select(s => new { s.StudentID, FullName = (s.FirstName ?? "") + " " + (s.LastName ?? "") })
                .ToListAsync();

            var sections = await _context.Sections
                .Include(s => s.GradeLevel)
                .Select(s => new {
                    s.SectionID,
                    DisplayName = (s.GradeLevel != null ? (s.GradeLevel.GradeName ?? "") : "") + " - Section " + (s.SectionName ?? "")
                })
                .ToListAsync();

            var classifications = await _context.AcademicClassifications
                .Select(c => new { c.ClassificationID, c.ClassificationName })
                .ToListAsync();

            ViewBag.Students = students;
            ViewBag.Sections = sections;
            ViewBag.Classifications = classifications;

            return View(enrollment);
        }
    }
}