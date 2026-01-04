using CompanyExpenses.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CompanyExpenses.Database.Configurations;

public class WorkplaceLimitConfiguration : IEntityTypeConfiguration<WorkplaceLimit>
{
    public void Configure(EntityTypeBuilder<WorkplaceLimit> builder)
    {
        builder.ToTable("WorkplaceLimits");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasDefaultValueSql("NEWID()");

        builder.Property(e => e.PeriodFrom)
            .IsRequired();

        builder.Property(e => e.PeriodTo)
            .IsRequired();

        builder.Property(e => e.LimitAmount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(e => e.Currency)
            .IsRequired()
            .HasMaxLength(3)
            .IsFixedLength();

        builder.Property(e => e.CreatedAt)
            .HasColumnType("datetime2(0)")
            .HasDefaultValueSql("SYSUTCDATETIME()");

        builder.Property(e => e.CreatedBy)
            .IsRequired()
            .HasMaxLength(450);

        // Relationship: WorkplaceLimit -> Workplace
        builder.HasOne(e => e.Workplace)
            .WithMany(w => w.Limits)
            .HasForeignKey(e => e.WorkplaceId)
            .OnDelete(DeleteBehavior.Cascade);

        // Relationship: WorkplaceLimit -> ExpenseCategory (optional)
        builder.HasOne(e => e.Category)
            .WithMany()
            .HasForeignKey(e => e.CategoryId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
