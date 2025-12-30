using CompanyExpenses.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CompanyExpenses.Database.Configurations;

public class ExpenseApprovalConfiguration : IEntityTypeConfiguration<ExpenseApproval>
{
    public void Configure(EntityTypeBuilder<ExpenseApproval> builder)
    {
        builder.ToTable("ExpenseApprovals");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasDefaultValueSql("NEWID()");

        builder.Property(e => e.Action)
            .IsRequired();

        builder.Property(e => e.ActorUserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(e => e.Note)
            .HasMaxLength(1000);

        builder.Property(e => e.CreatedAt)
            .HasColumnType("datetime2(0)")
            .HasDefaultValueSql("SYSUTCDATETIME()");

        builder.Property(e => e.CreatedBy)
            .IsRequired()
            .HasMaxLength(450);

        // Relationship: ExpenseApproval -> Expense
        builder.HasOne(e => e.Expense)
            .WithMany(exp => exp.Approvals)
            .HasForeignKey(e => e.ExpenseId)
            .OnDelete(DeleteBehavior.Cascade);

        // Index for queries
        builder.HasIndex(e => e.ExpenseId);
        builder.HasIndex(e => e.ActorUserId);
    }
}
