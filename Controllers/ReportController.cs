using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using EduTrack.Data;
using EduTrack.Models;
using System.Text.Json;

namespace EduTrack.Controllers
{
    public class ReportController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportController(ApplicationDbContext context)
        {
            _context = context;
        }

        private bool IsAuthorized()
        {
            var role = HttpContext.Session.GetString("Role");
            return role == "Admin" || role == "Principal";
        }

        // GET: Report/ExecutiveSummary
        public async Task<IActionResult> ExecutiveSummary()
        {
            if (!IsAuthorized()) return RedirectToAction("Login", "Account");

            try
            {
                // KPI Data
                ViewBag.TotalEnrollment = await _context.Enrollments.CountAsync(e => e.Status == "Active");
                ViewBag.TotalFaculty = await _context.Staff.CountAsync(s => s.StaffRole == "Teacher");

                var grades = await _context.Grades.Where(g => g.Score.HasValue).ToListAsync();
                var passingCount = grades.Count(g => (g.Score ?? 0) >= 75);
                var totalGrades = grades.Count;
                ViewBag.PassingRate = totalGrades > 0 ? Math.Round((double)passingCount / totalGrades * 100, 1) : 0;

                var totalBilled = await _context.Billings.SumAsync(b => (decimal?)b.Amount) ?? 0;
                var totalPaid = await _context.Payments.SumAsync(p => (decimal?)p.AmountPaid) ?? 0;
                ViewBag.CollectionRate = totalBilled > 0 ? Math.Round((double)(totalPaid / totalBilled * 100), 1) : 0;

                // Enrollment by Grade Level
                var enrollmentByGrade = await _context.Sections
                    .Include(s => s.GradeLevel)
                    .Select(s => new { GradeName = s.GradeLevel != null ? s.GradeLevel.GradeName : "Unknown", Count = s.Enrollments.Count(e => e.Status == "Active") })
                    .ToListAsync();

                var gradeLabels = enrollmentByGrade.Select(e => e.GradeName).ToList();
                var enrollmentData = enrollmentByGrade.Select(e => e.Count).ToList();

                ViewBag.GradeLabels = JsonSerializer.Serialize(gradeLabels);
                ViewBag.EnrollmentData = JsonSerializer.Serialize(enrollmentData);

                // Monthly Collections
                var monthlyData = await _context.Payments
                    .GroupBy(p => new { p.PaymentDate.Year, p.PaymentDate.Month })
                    .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
                    .Take(6)
                    .Select(g => new { MonthNumber = g.Key.Month, Total = g.Sum(p => p.AmountPaid) })
                    .ToListAsync();

                var monthLabels = monthlyData.Select(m => GetMonthName(m.MonthNumber)).ToList();
                var monthlyTotals = monthlyData.Select(m => m.Total).ToList();

                ViewBag.MonthLabels = JsonSerializer.Serialize(monthLabels);
                ViewBag.MonthlyCollections = JsonSerializer.Serialize(monthlyTotals);

                // Top Students
                var topStudents = await _context.Grades
                    .Where(g => g.Score.HasValue && g.Student != null)
                    .GroupBy(g => new { g.StudentID, StudentName = (g.Student.FirstName ?? "") + " " + (g.Student.LastName ?? "") })
                    .Select(g => new { g.Key.StudentName, Average = g.Average(x => x.Score ?? 0) })
                    .OrderByDescending(s => s.Average)
                    .Take(5)
                    .ToListAsync();

                ViewBag.TopStudents = topStudents.Select((s, i) => new { Rank = i + 1, s.StudentName, Average = Math.Round(s.Average, 1) }).ToList();

                // Attendance Summary
                var today = DateTime.Today;
                ViewBag.PresentCount = await _context.AttendanceStudents.CountAsync(a => a.Date == today && a.Status == "Present");
                ViewBag.AbsentCount = await _context.AttendanceStudents.CountAsync(a => a.Date == today && a.Status == "Absent");
                ViewBag.LateCount = await _context.AttendanceStudents.CountAsync(a => a.Date == today && a.Status == "Late");

                return View();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return View();
            }
        }

        // GET: Report/KPIDashboard
        public async Task<IActionResult> KPIDashboard()
        {
            if (!IsAuthorized()) return RedirectToAction("Login", "Account");

            try
            {
                ViewBag.RetentionRate = await CalculateRetentionRate();
                ViewBag.AttendanceRate = await CalculateAttendanceRate();
                ViewBag.AcademicPerformance = await CalculateAcademicPerformance();
                ViewBag.CollectionEfficiency = await CalculateCollectionEfficiency();

                return View();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return View();
            }
        }

        // GET: Report/StudentPerformance
        public async Task<IActionResult> StudentPerformance()
        {
            if (!IsAuthorized()) return RedirectToAction("Login", "Account");

            try
            {
                var students = await _context.Students
                    .Select(s => new
                    {
                        StudentName = (s.FirstName ?? "") + " " + (s.LastName ?? ""),
                        AverageGrade = _context.Grades.Where(g => g.StudentID == s.StudentID && g.Score.HasValue).Select(g => g.Score ?? 0).DefaultIfEmpty(0).Average(),
                        Status = s.Status ?? "Unknown"
                    })
                    .OrderByDescending(s => s.AverageGrade)
                    .ToListAsync();

                return View(students);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return View(new List<dynamic>());
            }
        }

        // GET: Report/FinancialSummary
        public async Task<IActionResult> FinancialSummary()
        {
            if (!IsAuthorized()) return RedirectToAction("Login", "Account");

            try
            {
                var monthlyCollections = await _context.Payments
                    .GroupBy(p => new { p.PaymentDate.Year, p.PaymentDate.Month })
                    .Select(g => new
                    {
                        Year = g.Key.Year,
                        MonthName = GetMonthName(g.Key.Month),
                        Total = g.Sum(p => p.AmountPaid)
                    })
                    .OrderByDescending(x => x.Year)
                    .ThenByDescending(x => x.MonthName)
                    .ToListAsync();

                var yearlyTotals = await _context.Payments
                    .GroupBy(p => p.PaymentDate.Year)
                    .Select(g => new
                    {
                        Year = g.Key,
                        Total = g.Sum(p => p.AmountPaid)
                    })
                    .OrderByDescending(x => x.Year)
                    .ToListAsync();

                var totalCollections = await _context.Payments.SumAsync(p => (decimal?)p.AmountPaid) ?? 0;
                var totalBillings = await _context.Billings.SumAsync(b => (decimal?)b.Amount) ?? 0;
                var outstandingBalance = totalBillings - totalCollections;

                ViewBag.TotalCollections = totalCollections.ToString("N2");
                ViewBag.TotalBillings = totalBillings.ToString("N2");
                ViewBag.OutstandingBalance = outstandingBalance.ToString("N2");
                ViewBag.YearlyTotals = yearlyTotals;

                return View(monthlyCollections);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                ViewBag.TotalCollections = "0.00";
                ViewBag.TotalBillings = "0.00";
                ViewBag.OutstandingBalance = "0.00";
                ViewBag.YearlyTotals = new List<dynamic>();
                return View(new List<dynamic>());
            }
        }

        private string GetMonthName(int month)
        {
            return System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month);
        }

        private async Task<double> CalculateRetentionRate()
        {
            var previousYear = await _context.Enrollments.CountAsync(e => e.SchoolYear == "2024-2025");
            var currentYear = await _context.Enrollments.CountAsync(e => e.SchoolYear == "2025-2026");
            return previousYear > 0 ? Math.Round((double)currentYear / previousYear * 100, 1) : 0;
        }

        private async Task<double> CalculateAttendanceRate()
        {
            var totalRecords = await _context.AttendanceStudents.CountAsync();
            var presentRecords = await _context.AttendanceStudents.CountAsync(a => a.Status == "Present");
            return totalRecords > 0 ? Math.Round((double)presentRecords / totalRecords * 100, 1) : 0;
        }

        private async Task<double> CalculateAcademicPerformance()
        {
            var grades = await _context.Grades.Where(g => g.Score.HasValue).ToListAsync();
            if (!grades.Any()) return 0;
            var avg = grades.Average(g => (double)(g.Score ?? 0));
            return Math.Round(avg, 1);
        }

        private async Task<double> CalculateCollectionEfficiency()
        {
            var totalBilled = await _context.Billings.SumAsync(b => (decimal?)b.Amount) ?? 0;
            var totalPaid = await _context.Payments.SumAsync(p => (decimal?)p.AmountPaid) ?? 0;
            return totalBilled > 0 ? Math.Round((double)(totalPaid / totalBilled * 100), 1) : 0;
        }
    }
}