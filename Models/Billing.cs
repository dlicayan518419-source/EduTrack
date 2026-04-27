using System.ComponentModel.DataAnnotations;

namespace EduTrack.Models
{
    public class Billing
    {
        [Key]
        public int BillingID { get; set; }
        public int StudentID { get; set; }
        public string SchoolYear { get; set; } = string.Empty;
        public string FeeType { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime DueDate { get; set; }
        public decimal Balance { get; set; }

        public Student Student { get; set; } = null!;
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
}