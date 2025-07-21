using DocumentProcessor.Dao.Entities;
using Microsoft.EntityFrameworkCore;

namespace DocumentProcessor.Dao;
public class DocumentDbContext : DbContext
{
    public DocumentDbContext(DbContextOptions<DocumentDbContext> options) : base(options)
    {
    }

    public DbSet<Process> Processes { get; set; } = null!;
    public DbSet<Entities.File> Files { get; set; } = null!;
    public DbSet<ProcessSagaState> ProcessSagaStates { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Process>(entity =>
        {
            entity.ToTable("Processes");

            entity.HasKey(p => p.ProcessId);

            entity.Property(p => p.Status)
                .IsRequired()
                .HasMaxLength(64);

            entity.Property(p => p.FolderPath)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(p => p.StartedAt)
                .HasColumnType("datetime(6)");

            entity.Property(p => p.CompletedAt)
                .HasColumnType("datetime(6)");

            entity.Property(p => p.CreatedAt)
                .HasColumnType("datetime(6)")
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)");

            entity.Property(p => p.UpdatedAt)
                .HasColumnType("datetime(6)")
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6)");

            entity.HasMany(p => p.Files)
                .WithOne(f => f.Process)
                .HasForeignKey(f => f.ProcessId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Entities.File>(entity =>
        {
            entity.ToTable("Files");

            entity.HasKey(f => f.FileId);

            entity.Property(f => f.FileName)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(f => f.Status)
                .IsRequired()
                .HasMaxLength(20);

            entity.Property(f => f.MostFrequentWords)
                .HasColumnType("TEXT");

            entity.Property(f => f.Summary)
                .HasColumnType("TEXT");

            entity.Property(f => f.ErrorMessage)
                .HasColumnType("TEXT");

            entity.Property(p => p.CreatedAt)
                .HasColumnType("datetime(6)")
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)");

            entity.Property(p => p.UpdatedAt)
                .HasColumnType("datetime(6)")
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6)");

            entity.HasIndex(f => f.ProcessId);
        });

        modelBuilder.Entity<ProcessSagaState>(entity =>
        {
            entity.ToTable("ProcessSagaStates");

            entity.HasKey(s => s.CorrelationId);

            entity.Property(s => s.CurrentState)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(p => p.CreatedAt)
                .HasColumnType("datetime(6)")
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)");

            entity.Property(p => p.UpdatedAt)
                .HasColumnType("datetime(6)")
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6)");

            entity.HasIndex(s => s.ProcessId);
        });
    }
}
