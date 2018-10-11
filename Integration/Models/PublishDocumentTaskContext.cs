using Microsoft.EntityFrameworkCore;

namespace Integration.Models
{
    public class PublishDocumentTaskContext : DbContext
    {
        public PublishDocumentTaskContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<PublishDocumentTask> PublishDocumentTasks { get; set; }

        public DbSet<PublishDocumentTaskAttempt> PublishDocumentTaskAttempts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PublishDocumentTask>(task =>
            {
                task.HasKey(w => w.Id);
                task.HasIndex(w => w.Id);
                task.Property(w => w.Id).ValueGeneratedNever();
                task.Ignore(w => w.IsFinished);
                task.HasMany(w => w.PublishDocumentTaskAttempts)
                    .WithOne(w => w.PublishDocumentTask)
                    .HasForeignKey(w => w.PublishDocumentTaskId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<PublishDocumentTaskAttempt>(taskAttempt =>
            {
                taskAttempt.HasKey(w => w.Id);
                taskAttempt.HasIndex(w => w.Id);
                taskAttempt.HasOne(w => w.PublishDocumentTask)
                    .WithMany(w => w.PublishDocumentTaskAttempts)
                    .HasForeignKey(w => w.PublishDocumentTaskId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}