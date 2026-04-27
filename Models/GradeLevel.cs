using System.ComponentModel.DataAnnotations;

namespace EduTrack.Models
{
    public class GradeLevel
    {
        [Key]
        public int GradeLevelID { get; set; }
        public string GradeName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public ICollection<Section> Sections { get; set; } = new List<Section>();
        public ICollection<Subject> Subjects { get; set; } = new List<Subject>();
    }
}