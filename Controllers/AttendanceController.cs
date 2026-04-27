using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using EduTrack.Data;
using EduTrack.Models;

namespace EduTrack.Controllers
{
    public class AttendanceController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AttendanceController(ApplicationDbContext context)
        {
            _context = context;
        }

        private bool IsTeacherOrAdmin()
        {
            var role = HttpContext.Session.GetString("Role");
            return role == "Admin" || role == "Principal" || role == "Teacher";
        }

        // GET: Attendance/Student
        public async Task<IActionResult> Student()
        {
            if (!IsTeacherOrAdmin()) return RedirectToAction("Login", "Account");

            try
            {
                var today = DateTime.Today;

                var attendances = await _context.AttendanceStudents
                    .Include(a => a.Student)
                    .Where(a => a.Date == today)
                    .ToListAsync();

                var allStudents = await _context.Students
                    .Where(s => s.Status == "Enrolled")
                    .Select(s => new { s.StudentID, FullName = (s.FirstName ?? "") + " " + (s.LastName ?? ""), s.Status })
                    .ToListAsync();

                ViewBag.Today = today;
                ViewBag.AllStudents = allStudents;
                ViewBag.Attendances = attendances;

                return View();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                ViewBag.AllStudents = new List<dynamic>();
                ViewBag.Attendances = new List<AttendanceStudent>();
                return View();
            }
        }

        // POST: Attendance/RecordStudent
        [HttpPost]
        public async Task<IActionResult> RecordStudent(int studentId, string status, string remarks)
        {
            if (!IsTeacherOrAdmin()) return RedirectToAction("Login", "Account");

            try
            {
                var today = DateTime.Today;
                var existing = await _context.AttendanceStudents
                    .FirstOrDefaultAsync(a => a.StudentID == studentId && a.Date == today);

                if (existing != null)
                {
                    existing.Status = status ?? "Present";
                    existing.Remarks = remarks ?? "";
                    _context.Update(existing);
                }
                else
                {
                    var attendance = new AttendanceStudent
                    {
                        StudentID = studentId,
                        Date = today,
                        Status = status ?? "Present",
                        Remarks = remarks ?? ""
                    };
                    _context.Add(attendance);
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving attendance: {ex.Message}");
            }

            return RedirectToAction(nameof(Student));
        }

        // GET: Attendance/Staff
        public async Task<IActionResult> Staff()
        {
            if (!IsTeacherOrAdmin()) return RedirectToAction("Login", "Account");

            try
            {
                var today = DateTime.Today;

                var attendances = await _context.AttendanceStaff
                    .Include(a => a.Staff)
                    .Where(a => a.Date == today)
                    .ToListAsync();

                var allStaff = await _context.Staff
                    .Select(s => new { s.StaffID, FullName = (s.FirstName ?? "") + " " + (s.LastName ?? ""), s.StaffRole })
                    .ToListAsync();

                ViewBag.Today = today;
                ViewBag.AllStaff = allStaff;
                ViewBag.Attendances = attendances;

                return View();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                ViewBag.AllStaff = new List<dynamic>();
                ViewBag.Attendances = new List<AttendanceStaff>();
                return View();
            }
        }

        // POST: Attendance/RecordStaff
        [HttpPost]
        public async Task<IActionResult> RecordStaff(int staffId, string status, TimeSpan? timeIn, TimeSpan? timeOut)
        {
            if (!IsTeacherOrAdmin()) return RedirectToAction("Login", "Account");

            try
            {
                var today = DateTime.Today;
                var existing = await _context.AttendanceStaff
                    .FirstOrDefaultAsync(a => a.StaffID == staffId && a.Date == today);

                if (existing != null)
                {
                    existing.Status = status ?? "Present";
                    existing.TimeIn = timeIn;
                    existing.TimeOut = timeOut;
                    _context.Update(existing);
                }
                else
                {
                    var attendance = new AttendanceStaff
                    {
                        StaffID = staffId,
                        Date = today,
                        Status = status ?? "Present",
                        TimeIn = timeIn,
                        TimeOut = timeOut
                    };
                    _context.Add(attendance);
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving staff attendance: {ex.Message}");
            }

            return RedirectToAction(nameof(Staff));
        }
    }
}