using System.ComponentModel.DataAnnotations;

namespace EduTrack.Models
{
    public class Staff
    {
        [Key]
        public int StaffID { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string StaffRole { get; set; } = string.Empty;
        public int? UserID { get; set; }
        public DateTime? HireDate { get; set; }

        public User? User { get; set; }
        public ICollection<Section> AdvisedSections { get; set; } = new List<Section>();
        public ICollection<Class> Classes { get; set; } = new List<Class>();
        public ICollection<AttendanceStaff> Attendances { get; set; } = new List<AttendanceStaff>();
        public ICollection<ConductRecord> ConductRecordsRecorded { get; set; } = new List<ConductRecord>();
    }
}