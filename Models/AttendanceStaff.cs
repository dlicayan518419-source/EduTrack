using System.ComponentModel.DataAnnotations;

namespace EduTrack.Models
{
    public class AttendanceStaff
    {
        [Key]
        public int AttendanceID { get; set; }
        public int StaffID { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan? TimeIn { get; set; }
        public TimeSpan? TimeOut { get; set; }
        public string Status { get; set; } = string.Empty;

        public Staff Staff { get; set; } = null!;
    }
}