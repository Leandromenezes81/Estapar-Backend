using Estapar.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Estapar.Infrastructure.Persistence.Configurations;

public sealed class SpotConfiguration : IEntityTypeConfiguration<Spot>
{
    public void Configure(EntityTypeBuilder<Spot> builder)
    {
        builder.ToTable("Spots");

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).ValueGeneratedNever();

        builder.Property(s => s.SectorName).HasMaxLength(50).IsRequired();
        builder.Property(s => s.Lat);
        builder.Property(s => s.Lng);
        builder.Property(s => s.Status).HasConversion<string>().HasMaxLength(20);

        builder.HasOne<Sector>()
            .WithMany()
            .HasForeignKey(s => s.SectorName)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(s => new { s.Lat, s.Lng });
    }
}
