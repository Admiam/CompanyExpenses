using CompanyExpenses.Models.Entities;
using CompanyExpenses.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CompanyExpenses.Database.Configurations;

public class InvitationConfiguration : IEntityTypeConfiguration<Invitation>
{
    public void Configure(EntityTypeBuilder<Invitation> builder)
    {
        builder.ToTable("Invitations");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasDefaultValueSql("NEWID()");

        builder.Property(e => e.Email)
            .IsRequired()
            .HasMaxLength(320);

        builder.Property(e => e.InvitedRoleId)
            .HasMaxLength(450);

        builder.Property(e => e.Token)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.ExpiresAt)
            .HasColumnType("datetime2(0)")
            .IsRequired();

        builder.Property(e => e.AcceptedAt)
            .HasColumnType("datetime2(0)");

        builder.Property(e => e.InvitedByUserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(e => e.Status)
            .HasDefaultValue(InvitationStatus.Pending);

        builder.Property(e => e.CreatedAt)
            .HasColumnType("datetime2(0)")
            .HasDefaultValueSql("SYSUTCDATETIME()");

        builder.Property(e => e.CreatedBy)
            .IsRequired()
            .HasMaxLength(450);

        // Relationship: Invitation -> Workplace (optional)
        builder.HasOne(e => e.Workplace)
            .WithMany(w => w.Invitations)
            .HasForeignKey(e => e.WorkplaceId)
            .OnDelete(DeleteBehavior.SetNull);

        // Unique token
        builder.HasIndex(e => e.Token).IsUnique();

        // Index for queries
        builder.HasIndex(e => e.Email);
        builder.HasIndex(e => e.Status);
    }
}
