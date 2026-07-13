using Estapar.ParkingManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Estapar.ParkingManager.Infrastructure.Data.Persistence.Configurations;

/// <summary>Mapeamento do EF Core para a entidade <see cref="Spot"/>.</summary>
public sealed class SpotConfiguration : IEntityTypeConfiguration<Spot>
{
    /// <summary>Configura a tabela, chave, propriedades, relacionamento com <see cref="Sector"/> e índices de <see cref="Spot"/>.</summary>
    public void Configure(EntityTypeBuilder<Spot> builder)
    {
        builder.ToTable("Spots");

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).ValueGeneratedNever();

        builder.Property(s => s.SectorId).IsRequired();
        builder.Property(s => s.Lat);
        builder.Property(s => s.Lng);
        builder.Property(s => s.Status).HasConversion<string>().HasMaxLength(20);

        builder.HasOne<Sector>()
            .WithMany()
            .HasForeignKey(s => s.SectorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(s => new { s.Lat, s.Lng });
        builder.HasIndex(s => s.SectorId);
    }
}
