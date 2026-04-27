using System.ComponentModel.DataAnnotations;

namespace EduTrack.Models
{
    public class ConductRecord
    {
        [Key]
        public int ConductID { get; set; }
        public int StudentID { get; set; }
        public DateTime Date { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ActionTaken { get; set; } = string.Empty;
        public int RecordedBy { get; set; }

        public Student Student { get; set; } = null!;
        public Staff Recorder { get; set; } = null!;
    }
}