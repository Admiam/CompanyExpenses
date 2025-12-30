using CompanyExpenses.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CompanyExpenses.Database.Configurations;

public class WorkplaceMemberConfiguration : IEntityTypeConfiguration<WorkplaceMember>
{
    public void Configure(EntityTypeBuilder<WorkplaceMember> builder)
    {
        builder.ToTable("WorkplaceMembers");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasDefaultValueSql("NEWID()");

        builder.Property(e => e.UserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(e => e.PositionName)
            .HasMaxLength(200);

        builder.Property(e => e.IsManager)
            .HasDefaultValue(false);

        builder.Property(e => e.CreatedAt)
            .HasColumnType("datetime2(0)")
            .HasDefaultValueSql("SYSUTCDATETIME()");

        builder.Property(e => e.CreatedBy)
            .IsRequired()
            .HasMaxLength(450);

        // Relationship: WorkplaceMember -> Workplace
        builder.HasOne(e => e.Workplace)
            .WithMany(w => w.Members)
            .HasForeignKey(e => e.WorkplaceId)
            .OnDelete(DeleteBehavior.Cascade);

        // Unique constraint: one user per workplace
        builder.HasIndex(e => new { e.WorkplaceId, e.UserId }).IsUnique();
    }
}
