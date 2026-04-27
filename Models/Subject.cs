using System.ComponentModel.DataAnnotations;

namespace EduTrack.Models
{
    public class Subject
    {
        [Key]
        public int SubjectID { get; set; }
        public string SubjectName { get; set; } = string.Empty;
        public int GradeLevelID { get; set; }

        public GradeLevel GradeLevel { get; set; } = null!;
        public ICollection<Class> Classes { get; set; } = new List<Class>();
    }
}