using System.ComponentModel.DataAnnotations;

namespace EduTrack.Models
{
    public class Enrollment
    {
        [Key]
        public int EnrollmentID { get; set; }
        public int StudentID { get; set; }
        public int SectionID { get; set; }
        public string SchoolYear { get; set; } = string.Empty;
        public DateTime EnrollmentDate { get; set; }
        public string Status { get; set; } = "Active";
        public int? ClassificationID { get; set; }

        public Student Student { get; set; } = null!;
        public Section Section { get; set; } = null!;
        public AcademicClassification? Classification { get; set; }
    }
}