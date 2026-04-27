using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduTrack.Models
{
    public class Grade
    {
        [Key]
        public int GradeID { get; set; }

        [ForeignKey("Student")]
        public int StudentID { get; set; }

        [ForeignKey("Class")]
        public int ClassID { get; set; }

        public byte Quarter { get; set; }  // Changed from int to byte (matches tinyint)

        [Column(TypeName = "decimal(5,2)")]
        public decimal? Score { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal? GeneralAverage { get; set; }

        // Navigation properties
        public Student Student { get; set; } = null!;
        public Class Class { get; set; } = null!;
    }
}