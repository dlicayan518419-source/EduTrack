using System.ComponentModel.DataAnnotations;

namespace EduTrack.Models
{
    public class Section
    {
        [Key]
        public int SectionID { get; set; }
        public int GradeLevelID { get; set; }
        public string SectionName { get; set; } = string.Empty;
        public int? AdviserID { get; set; }

        public GradeLevel GradeLevel { get; set; } = null!;
        public Staff? Adviser { get; set; }
        public ICollection<Class> Classes { get; set; } = new List<Class>();
        public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    }
}