using CompanyExpenses.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CompanyExpenses.Database.Configurations;

public class ExpenseAttachmentConfiguration : IEntityTypeConfiguration<ExpenseAttachment>
{
    public void Configure(EntityTypeBuilder<ExpenseAttachment> builder)
    {
        builder.ToTable("ExpenseAttachments");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasDefaultValueSql("NEWID()");

        builder.Property(e => e.OriginalFileName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(e => e.StoredFileName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(e => e.DataType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.FileSize)
            .IsRequired();

        builder.Property(e => e.UploadedByUserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(e => e.UploadedAt)
            .HasColumnType("datetime2(0)")
            .HasDefaultValueSql("SYSUTCDATETIME()");

        // Relationship: ExpenseAttachment -> Expense
        builder.HasOne(e => e.Expense)
            .WithMany(exp => exp.Attachments)
            .HasForeignKey(e => e.ExpenseId)
            .OnDelete(DeleteBehavior.Cascade);

        // Index for queries
        builder.HasIndex(e => e.ExpenseId);
    }
}
