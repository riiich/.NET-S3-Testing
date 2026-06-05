using Microsoft.EntityFrameworkCore;
using server.Models;

namespace server.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();

    public DbSet<StoredS3File> StoredS3Files => Set<StoredS3File>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(user => user.Id);
            entity.Property(user => user.Name).HasMaxLength(150).IsRequired();
            entity.Property(user => user.Email).HasMaxLength(255).IsRequired();
            entity.Property(user => user.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            entity.HasIndex(user => user.Email).IsUnique();
        });

        modelBuilder.Entity<StoredS3File>(entity =>
        {
            entity.HasKey(item => item.Id);
            entity.Property(item => item.S3Key).HasMaxLength(1024).IsRequired();
            entity.Property(item => item.Status).HasConversion<int>();
            entity.Property(item => item.FileName).HasMaxLength(255).IsRequired();
            entity.Property(item => item.MimeType).HasMaxLength(255).IsRequired();
            entity.Property(item => item.UploadedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            entity.HasIndex(item => item.UserId);
            entity.HasIndex(item => item.S3Key).IsUnique();

            entity.HasOne(item => item.User)
                .WithMany(user => user.StoredS3Files)
                .HasForeignKey(item => item.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
