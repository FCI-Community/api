using GraduationProject.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Graduation_project.Data
{
    public class AppDbContext:IdentityDbContext<AppUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        // DbSets for application models
        public DbSet<Block> Blocks { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<StudentProfile> StudentProfiles { get; set; }
        public DbSet<SubjectStaff> SubjectStaffs { get; set; }
        public DbSet<SupportTicket> SupportTickets { get; set; }
        public DbSet<Major> Majors { get; set; }
        public DbSet<Subject> Subjects { get; set; }
        public DbSet<GlobalAnnouncement> GlobalAnnouncements { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Message>(entity =>
            {
                entity.HasKey(m => m.Id);
                entity.Property(m => m.Content).IsRequired();
                entity.Property(m => m.SentAt).HasDefaultValueSql("GETUTCDATE()");

                entity.HasOne(m => m.Sender)
                      .WithMany(u => u.SentMessages)
                      .HasForeignKey(m => m.SenderId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(m => m.Receiver)
                      .WithMany(u => u.ReceivedMessages)
                      .HasForeignKey(m => m.ReceiverId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<Block>(entity =>
            {
                entity.HasKey(b => b.Id);

                entity.HasOne(b => b.Blocker)
                      .WithMany(u => u.BlocksInitiated)
                      .HasForeignKey(b => b.BlockerId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(b => b.Blocked)
                      .WithMany(u => u.BlocksReceived)
                      .HasForeignKey(b => b.BlockedId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<StudentProfile>(entity =>
            {
                entity.HasKey(sp => sp.AppUserId);

                entity.HasOne(sp => sp.AppUser)
                      .WithOne(u => u.StudentProfile)
                      .HasForeignKey<StudentProfile>(sp => sp.AppUserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(sp => sp.StudentId).IsUnique();

                entity.HasOne(sp => sp.Major)
                      .WithMany(m => m.StudentProfiles)
                      .HasForeignKey(sp => sp.MajorId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<SubjectStaff>(entity =>
            {
                entity.HasKey(ss => ss.Id);

                entity.HasOne(ss => ss.AppUser)
                      .WithMany(u => u.SubjectsStaff)
                      .HasForeignKey(ss => ss.AppUserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(ss => ss.Subject)
                      .WithMany(s => s.SubjectStaff)
                      .HasForeignKey(ss => ss.SubjectId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(ss => new { ss.AppUserId, ss.SubjectId });
            });

            
            builder.Entity<SupportTicket>(entity =>
            {
                entity.HasKey(t => t.Id);
                entity.Property(t => t.Title).IsRequired();

                entity.HasOne(t => t.AppUser)
                      .WithMany(u => u.SupportTickets)
                      .HasForeignKey(t => t.AppUserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<Subject>(entity =>
            {
                entity.HasKey(s => s.Id);
                entity.Property(s => s.Name).IsRequired().HasMaxLength(200);
                entity.Property(s => s.Code).IsRequired().HasMaxLength(50);
            });

            builder.Entity<Major>(entity =>
            {
                entity.HasKey(m => m.Id);
                entity.Property(m => m.Name).IsRequired().HasMaxLength(200);
            });

            builder.Entity<GlobalAnnouncement>(entity =>
            {
                entity.HasKey(g => g.Id);
                entity.Property(g => g.Title).IsRequired().HasMaxLength(300);
                entity.Property(g => g.Content).IsRequired();
                entity.Property(g => g.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            });
        }
    }
}
