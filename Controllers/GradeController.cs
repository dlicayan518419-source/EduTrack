using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using EduTrack.Data;
using EduTrack.Models;

namespace EduTrack.Controllers
{
    public class GradeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public GradeController(ApplicationDbContext context)
        {
            _context = context;
        }

        private bool IsTeacher()
        {
            var role = HttpContext.Session.GetString("Role");
            return role == "Admin" || role == "Principal" || role == "Teacher";
        }

        // GET: Grade
        public async Task<IActionResult> Index()
        {
            if (!IsTeacher()) return RedirectToAction("Login", "Account");

            try
            {
                var username = HttpContext.Session.GetString("Username");
                var teacher = await _context.Staff
                    .FirstOrDefaultAsync(s => s.User != null && s.User.Username == username);

                var grades = await _context.Grades
                    .Include(g => g.Student)
                    .Include(g => g.Class)
                        .ThenInclude(c => c != null ? c.Subject : null)
                    .Include(g => g.Class)
                        .ThenInclude(c => c != null ? c.Section : null)
                            .ThenInclude(s => s != null ? s.GradeLevel : null)
                    .Where(g => teacher == null || g.Class.TeacherID == teacher.StaffID)
                    .Select(g => new {
                        Grade = g,
                        StudentName = g.Student != null ? (g.Student.FirstName ?? "") + " " + (g.Student.LastName ?? "") : "Unknown",
                        SubjectName = g.Class != null && g.Class.Subject != null ? (g.Class.Subject.SubjectName ?? "") : "Unknown",
                        GradeLevel = g.Class != null && g.Class.Section != null && g.Class.Section.GradeLevel != null ? (g.Class.Section.GradeLevel.GradeName ?? "") : "Unknown",
                        SectionName = g.Class != null && g.Class.Section != null ? (g.Class.Section.SectionName ?? "") : "Unknown"
                    })
                    .ToListAsync();

                return View(grades);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading grades: {ex.Message}");
                return View(new List<dynamic>());
            }
        }

        // GET: Grade/EnterGrades
        public async Task<IActionResult> EnterGrades()
        {
            if (!IsTeacher()) return RedirectToAction("Login", "Account");

            try
            {
                var username = HttpContext.Session.GetString("Username");
                var teacher = await _context.Staff
                    .FirstOrDefaultAsync(s => s.User != null && s.User.Username == username);

                if (teacher == null)
                {
                    ViewBag.Classes = new List<Class>();
                    return View();
                }

                var classes = await _context.Classes
                    .Include(c => c.Subject)
                    .Include(c => c.Section)
                        .ThenInclude(s => s.GradeLevel)
                    .Where(c => c.TeacherID == teacher.StaffID)
                    .Select(c => new {
                        c.ClassID,
                        SubjectName = c.Subject != null ? (c.Subject.SubjectName ?? "") : "Unknown",
                        GradeName = c.Section != null && c.Section.GradeLevel != null ? (c.Section.GradeLevel.GradeName ?? "") : "Unknown",
                        SectionName = c.Section != null ? (c.Section.SectionName ?? "") : "Unknown"
                    })
                    .ToListAsync();

                ViewBag.Classes = classes;
                return View();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading classes: {ex.Message}");
                ViewBag.Classes = new List<dynamic>();
                return View();
            }
        }

        // POST: Grade/SaveGrades
        [HttpPost]
        public async Task<IActionResult> SaveGrades(int classId, byte quarter, List<int> studentIds, List<decimal> scores)
        {
            if (!IsTeacher()) return RedirectToAction("Login", "Account");

            if (studentIds == null || scores == null || studentIds.Count != scores.Count)
            {
                return RedirectToAction(nameof(EnterGrades));
            }

            try
            {
                for (int i = 0; i < studentIds.Count; i++)
                {
                    var existingGrade = await _context.Grades
                        .FirstOrDefaultAsync(g => g.StudentID == studentIds[i] && g.ClassID == classId && g.Quarter == quarter);

                    if (existingGrade != null)
                    {
                        existingGrade.Score = scores[i];
                        _context.Update(existingGrade);
                    }
                    else
                    {
                        var grade = new Grade
                        {
                            StudentID = studentIds[i],
                            ClassID = classId,
                            Quarter = quarter,
                            Score = scores[i]
                        };
                        _context.Add(grade);
                    }
                }
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving grades: {ex.Message}");
            }

            return RedirectToAction(nameof(EnterGrades));
        }

        // GET: Grade/ClassStudents
        public async Task<IActionResult> ClassStudents(int classId, byte quarter)
        {
            if (!IsTeacher()) return RedirectToAction("Login", "Account");

            try
            {
                var classInfo = await _context.Classes
                    .Include(c => c.Subject)
                    .Include(c => c.Section)
                        .ThenInclude(s => s.GradeLevel)
                    .FirstOrDefaultAsync(c => c.ClassID == classId);

                if (classInfo == null)
                {
                    return NotFound();
                }

                var enrollments = await _context.Enrollments
                    .Include(e => e.Student)
                    .Where(e => e.SectionID == classInfo.SectionID && e.Status == "Active")
                    .ToListAsync();

                var students = enrollments
                    .Where(e => e.Student != null)
                    .Select(e => new {
                        e.Student.StudentID,
                        StudentName = (e.Student.FirstName ?? "") + " " + (e.Student.LastName ?? "")
                    })
                    .ToList();

                var grades = new Dictionary<int, decimal?>();

                foreach (var student in students)
                {
                    var grade = await _context.Grades
                        .FirstOrDefaultAsync(g => g.StudentID == student.StudentID && g.ClassID == classId && g.Quarter == quarter);
                    grades[student.StudentID] = grade?.Score;
                }

                ViewBag.Class = classInfo;
                ViewBag.Quarter = quarter;
                ViewBag.Students = students;
                ViewBag.Grades = grades;

                return View();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading class students: {ex.Message}");
                return RedirectToAction(nameof(EnterGrades));
            }
        }
    }
}