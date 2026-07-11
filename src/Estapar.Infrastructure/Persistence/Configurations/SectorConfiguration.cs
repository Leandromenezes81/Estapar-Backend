using Estapar.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Estapar.Infrastructure.Persistence.Configurations;

public sealed class SectorConfiguration : IEntityTypeConfiguration<Sector>
{
    public void Configure(EntityTypeBuilder<Sector> builder)
    {
        builder.ToTable("Sectors");

        builder.HasKey(s => s.Name);
        builder.Property(s => s.Name).HasMaxLength(50);

        builder.Property(s => s.BasePrice).HasColumnType("decimal(18,2)");
        builder.Property(s => s.MaxCapacity);
    }
}
