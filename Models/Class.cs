using System.ComponentModel.DataAnnotations;

namespace EduTrack.Models
{
    public class Class
    {
        [Key]
        public int ClassID { get; set; }
        public int SectionID { get; set; }
        public int SubjectID { get; set; }
        public int TeacherID { get; set; }

        public Section Section { get; set; } = null!;
        public Subject Subject { get; set; } = null!;
        public Staff Teacher { get; set; } = null!;
        public ICollection<Grade> Grades { get; set; } = new List<Grade>();
    }
}