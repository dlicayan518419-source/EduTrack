using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using EduTrack.Data;
using EduTrack.Models;

namespace EduTrack.Controllers
{
    public class StudentController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StudentController(ApplicationDbContext context)
        {
            _context = context;
        }

        private bool IsAuthorized()
        {
            var role = HttpContext.Session.GetString("Role");
            return role == "Admin" || role == "Principal" || role == "Registrar";
        }

        // GET: Student
        public async Task<IActionResult> Index()
        {
            if (!IsAuthorized()) return RedirectToAction("Login", "Account");
            var students = await _context.Students.ToListAsync();
            return View(students);
        }

        // GET: Student/Details/5
        public async Task<IActionResult> Details(int id)
        {
            if (!IsAuthorized()) return RedirectToAction("Login", "Account");
            var student = await _context.Students
                .Include(s => s.Enrollments)
                .ThenInclude(e => e.Section)
                .ThenInclude(sec => sec.GradeLevel)
                .FirstOrDefaultAsync(s => s.StudentID == id);
            if (student == null) return NotFound();
            return View(student);
        }

        // GET: Student/Create
        public IActionResult Create()
        {
            if (!IsAuthorized()) return RedirectToAction("Login", "Account");
            return View();
        }

        // POST: Student/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Student student)
        {
            if (!IsAuthorized()) return RedirectToAction("Login", "Account");
            if (ModelState.IsValid)
            {
                student.Status = "Enrolled";
                _context.Add(student);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(student);
        }

        // GET: Student/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            if (!IsAuthorized()) return RedirectToAction("Login", "Account");
            var student = await _context.Students.FindAsync(id);
            if (student == null) return NotFound();
            return View(student);
        }

        // GET: Student/Update/5
        public async Task<IActionResult> Update(int id)
        {
            if (!IsAuthorized()) return RedirectToAction("Login", "Account");
            var student = await _context.Students.FindAsync(id);
            if (student == null) return NotFound();
            return View(student);
        }

        // POST: Student/Update/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int id, Student student)
        {
            if (!IsAuthorized()) return RedirectToAction("Login", "Account");
            if (id != student.StudentID) return NotFound();
            if (ModelState.IsValid)
            {
                _context.Update(student);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(student);
        }

        // POST: Student/Archive/5 (soft delete - change status)
        [HttpPost, ActionName("Archive")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ArchiveConfirmed(int id)
        {
            if (!IsAuthorized()) return RedirectToAction("Login", "Account");
            var student = await _context.Students.FindAsync(id);
            if (student != null)
            {
                student.Status = "Transferred";
                _context.Update(student);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}