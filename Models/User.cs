using System.ComponentModel.DataAnnotations;

namespace EduTrack.Models
{
    public class User
    {
        [Key]
        public int UserID { get; set; }
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int RoleID { get; set; }
        public DateTime? LastLogin { get; set; }
        public bool IsActive { get; set; } = true;

        public Role Role { get; set; } = null!;
        public Student? Student { get; set; }
        public Staff? Staff { get; set; }
        public ICollection<SystemLog> SystemLogs { get; set; } = new List<SystemLog>();
    }
}