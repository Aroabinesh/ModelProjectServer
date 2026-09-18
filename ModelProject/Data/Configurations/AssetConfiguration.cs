using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ModelProject.Entities;

namespace ModelProject.Data.Configurations;

public class AssetConfiguration : IEntityTypeConfiguration<Asset>
{
    public void Configure(EntityTypeBuilder<Asset> builder)
    {
        builder.ToTable("Assets");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.AssetCode)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(a => a.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(a => a.AssetType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.Status)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.HasOne(a => a.Facility)
            .WithMany(f => f.Assets)
            .HasForeignKey(a => a.FacilityId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(a => a.FacilityId);
        builder.HasIndex(a => a.AssetCode).IsUnique();
    }
}
