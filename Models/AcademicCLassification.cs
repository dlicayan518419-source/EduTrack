using System.ComponentModel.DataAnnotations;

namespace EduTrack.Models
{
    public class AcademicClassification
    {
        [Key]  
        public int ClassificationID { get; set; }
        public string ClassificationName { get; set; } = string.Empty;
        public decimal MinAverage { get; set; }
        public decimal MaxAverage { get; set; }

        public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    }
}