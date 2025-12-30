using CompanyExpenses.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CompanyExpenses.Database.Configurations;

public class ExpenseConfiguration : IEntityTypeConfiguration<Expense>
{
    public void Configure(EntityTypeBuilder<Expense> builder)
    {
        builder.ToTable("Expenses");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasDefaultValueSql("NEWID()");

        builder.Property(e => e.EmployeeUserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(e => e.Amount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(e => e.Currency)
            .IsRequired()
            .HasMaxLength(3)
            .IsFixedLength();

        builder.Property(e => e.ExpenseDate)
            .IsRequired();

        builder.Property(e => e.Description)
            .HasMaxLength(1000);

        builder.Property(e => e.Status)
            .HasDefaultValue(Models.Enums.ExpenseStatus.Pending);

        builder.Property(e => e.SubmittedAt)
            .HasColumnType("datetime2(0)")
            .HasDefaultValueSql("SYSUTCDATETIME()");

        builder.Property(e => e.LastDecisionAt)
            .HasColumnType("datetime2(0)");

        builder.Property(e => e.LastDecisionBy)
            .HasMaxLength(450);

        builder.Property(e => e.RejectionNote)
            .HasMaxLength(1000);

        builder.Property(e => e.IsDeleted)
            .HasDefaultValue(false);

        builder.Property(e => e.CreatedAt)
            .HasColumnType("datetime2(0)")
            .HasDefaultValueSql("SYSUTCDATETIME()");

        builder.Property(e => e.CreatedBy)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(e => e.UpdatedAt)
            .HasColumnType("datetime2(0)")
            .HasDefaultValueSql("SYSUTCDATETIME()");

        // Relationship: Expense -> Workplace
        builder.HasOne(e => e.Workplace)
            .WithMany(w => w.Expenses)
            .HasForeignKey(e => e.WorkplaceId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relationship: Expense -> ExpenseCategory
        builder.HasOne(e => e.Category)
            .WithMany(c => c.Expenses)
            .HasForeignKey(e => e.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // Query filter for soft delete
        builder.HasQueryFilter(e => !e.IsDeleted);

        // Index for common queries
        builder.HasIndex(e => e.EmployeeUserId);
        builder.HasIndex(e => e.WorkplaceId);
        builder.HasIndex(e => e.Status);
        builder.HasIndex(e => e.ExpenseDate);
    }
}
