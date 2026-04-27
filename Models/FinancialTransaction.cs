using System.ComponentModel.DataAnnotations;

namespace EduTrack.Models
{
    public class FinancialTransaction
    {
        [Key]
        public int TransactionID { get; set; }
        public DateTime Date { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Description { get; set; } = string.Empty;
    }
}