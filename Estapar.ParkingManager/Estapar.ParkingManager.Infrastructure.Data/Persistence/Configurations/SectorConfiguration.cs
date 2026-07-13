using Estapar.ParkingManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Estapar.ParkingManager.Infrastructure.Data.Persistence.Configurations;

/// <summary>Mapeamento do EF Core para a entidade <see cref="Sector"/>.</summary>
public sealed class SectorConfiguration : IEntityTypeConfiguration<Sector>
{
    /// <summary>Configura a tabela, chave, propriedades e índices de <see cref="Sector"/>.</summary>
    public void Configure(EntityTypeBuilder<Sector> builder)
    {
        builder.ToTable("Sectors");

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).ValueGeneratedNever();

        builder.Property(s => s.Name).HasMaxLength(50).IsRequired();
        builder.HasIndex(s => s.Name);

        builder.Property(s => s.BasePrice).HasColumnType("decimal(18,2)");
        builder.Property(s => s.MaxCapacity);
    }
}
