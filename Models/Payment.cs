using System.ComponentModel.DataAnnotations;

namespace EduTrack.Models
{
    public class Payment
    {
        [Key]
        public int PaymentID { get; set; }
        public int BillingID { get; set; }
        public DateTime PaymentDate { get; set; }
        public decimal AmountPaid { get; set; }
        public string ReferenceNumber { get; set; } = string.Empty;
        public string ReceiptNumber { get; set; } = string.Empty;

        public Billing Billing { get; set; } = null!;
    }
}