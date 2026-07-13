using Estapar.Garage.Api.Domain.Entities;
using Estapar.Garage.Api.Infrastructure.Persistence.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Estapar.Garage.Api.Infrastructure.Persistence.Configurations;

/// <summary>Mapeamento do EF Core para a entidade <see cref="Spot"/>.</summary>
public sealed class SpotConfiguration : IEntityTypeConfiguration<Spot>
{
    /// <summary>Configura a tabela, chave, propriedades, filtro de soft delete, relacionamento com <see cref="Sector"/> e dados de seed de <see cref="Spot"/>.</summary>
    public void Configure(EntityTypeBuilder<Spot> builder)
    {
        builder.ToTable("Spots");

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Code).HasMaxLength(20).IsRequired();
        builder.Property(s => s.Lat);
        builder.Property(s => s.Lng);
        builder.Property(s => s.IsDeleted);

        builder.HasQueryFilter(s => !s.IsDeleted);

        builder.HasOne(sp => sp.Sector)
            .WithMany()
            .HasForeignKey(sp => sp.SectorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasData(GarageSeedData.Spots());
    }
}
