using System.ComponentModel.DataAnnotations;

namespace EduTrack.Models
{
    public class AttendanceStudent
    {
        [Key]
        public int AttendanceID { get; set; }
        public int StudentID { get; set; }
        public DateTime Date { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Remarks { get; set; } = string.Empty;

        public Student Student { get; set; } = null!;
    }
}