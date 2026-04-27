using Microsoft.EntityFrameworkCore;
using EduTrack.Models;

namespace EduTrack.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Role> Roles { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<Staff> Staff { get; set; }
        public DbSet<GradeLevel> GradeLevels { get; set; }
        public DbSet<Section> Sections { get; set; }
        public DbSet<Subject> Subjects { get; set; }
        public DbSet<Class> Classes { get; set; }
        public DbSet<Enrollment> Enrollments { get; set; }
        public DbSet<Grade> Grades { get; set; }
        public DbSet<AcademicClassification> AcademicClassifications { get; set; }
        public DbSet<AttendanceStudent> AttendanceStudents { get; set; }
        public DbSet<AttendanceStaff> AttendanceStaff { get; set; }
        public DbSet<ConductRecord> ConductRecords { get; set; }
        public DbSet<HealthRecord> HealthRecords { get; set; }
        public DbSet<Billing> Billings { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<FinancialTransaction> FinancialTransactions { get; set; }
        public DbSet<SystemLog> SystemLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>().ToTable("user");
            modelBuilder.Entity<Role>().ToTable("role");
            modelBuilder.Entity<Student>().ToTable("student");
            modelBuilder.Entity<Staff>().ToTable("staff");
            modelBuilder.Entity<GradeLevel>().ToTable("gradelevel");
            modelBuilder.Entity<Section>().ToTable("section");
            modelBuilder.Entity<Subject>().ToTable("subject");
            modelBuilder.Entity<Class>().ToTable("class");
            modelBuilder.Entity<Enrollment>().ToTable("enrollment");
            modelBuilder.Entity<Grade>().ToTable("grade");
            modelBuilder.Entity<AcademicClassification>().ToTable("academicclassification");
            modelBuilder.Entity<AttendanceStudent>().ToTable("attendancestudent");
            modelBuilder.Entity<AttendanceStaff>().ToTable("attendancestaff");
            modelBuilder.Entity<ConductRecord>().ToTable("conductrecord");
            modelBuilder.Entity<HealthRecord>().ToTable("healthrecord");
            modelBuilder.Entity<Billing>().ToTable("billing");
            modelBuilder.Entity<Payment>().ToTable("payment");
            modelBuilder.Entity<FinancialTransaction>().ToTable("financialtransaction");
            modelBuilder.Entity<SystemLog>().ToTable("systemlog");
        }
    }
}