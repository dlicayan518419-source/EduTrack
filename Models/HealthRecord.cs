using System.ComponentModel.DataAnnotations;

namespace EduTrack.Models
{
    public class HealthRecord
    {
        [Key]
        public int HealthRecordID { get; set; }
        public int StudentID { get; set; }
        public DateTime Date { get; set; }
        public string VisitType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Diagnosis { get; set; } = string.Empty;
        public string FollowUp { get; set; } = string.Empty;

        public Student Student { get; set; } = null!;
    }
}