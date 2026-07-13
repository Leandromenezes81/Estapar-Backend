using Estapar.Garage.Api.Domain.Entities;
using Estapar.Garage.Api.Infrastructure.Persistence.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Estapar.Garage.Api.Infrastructure.Persistence.Configurations;

/// <summary>Mapeamento do EF Core para a entidade <see cref="Sector"/>.</summary>
public sealed class SectorConfiguration : IEntityTypeConfiguration<Sector>
{
    /// <summary>Configura a tabela, chave, propriedades, filtro de soft delete e dados de seed de <see cref="Sector"/>.</summary>
    public void Configure(EntityTypeBuilder<Sector> builder)
    {
        builder.ToTable("Sectors");

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Name).HasMaxLength(50).IsRequired();
        builder.Property(s => s.BasePrice).HasColumnType("decimal(18,2)");
        builder.Property(s => s.MaxCapacity);
        builder.Property(s => s.IsDeleted);

        builder.HasQueryFilter(s => !s.IsDeleted);

        builder.HasData(GarageSeedData.Sectors());
    }
}
