using System.ComponentModel.DataAnnotations;

namespace EduTrack.Models
{
    public class SystemLog
    {
        [Key]
        public int LogID { get; set; }
        public int UserID { get; set; }
        public string Action { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string IPAddress { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;

        public User User { get; set; } = null!;
    }
}