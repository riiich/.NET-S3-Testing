using Microsoft.EntityFrameworkCore;
using server.Models;

namespace server.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<S3Metadata> S3Metadata => Set<S3Metadata>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<S3Metadata>(entity =>
        {
            entity.HasKey(item => item.Id);
            entity.Property(item => item.OwnerId).HasMaxLength(255).IsRequired();
            entity.Property(item => item.S3Key).HasMaxLength(1024).IsRequired();
            entity.Property(item => item.Status).HasConversion<int>();
            entity.Property(item => item.FileName).HasMaxLength(255).IsRequired();
            entity.Property(item => item.MimeType).HasMaxLength(255).IsRequired();
            entity.Property(item => item.UploadedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            entity.HasIndex(item => item.OwnerId);
            entity.HasIndex(item => item.S3Key).IsUnique();
        });
    }
}