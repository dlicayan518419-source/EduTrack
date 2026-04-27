using System.ComponentModel.DataAnnotations;

namespace EduTrack.Models
{
    public class Student
    {
        [Key]
        public int StudentID { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public DateTime? DateOfBirth { get; set; }
        public string Gender { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Status { get; set; } = "Enrolled";
        public int? UserID { get; set; }

        public User? User { get; set; }
        public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
        public ICollection<Grade> Grades { get; set; } = new List<Grade>();
        public ICollection<AttendanceStudent> Attendances { get; set; } = new List<AttendanceStudent>();
        public ICollection<ConductRecord> ConductRecords { get; set; } = new List<ConductRecord>();
        public ICollection<HealthRecord> HealthRecords { get; set; } = new List<HealthRecord>();
        public ICollection<Billing> Billings { get; set; } = new List<Billing>();
    }
}