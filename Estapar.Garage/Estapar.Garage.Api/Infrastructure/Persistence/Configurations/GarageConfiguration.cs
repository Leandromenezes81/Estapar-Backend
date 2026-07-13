using Estapar.Garage.Api.Infrastructure.Persistence.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using GarageEntity = Estapar.Garage.Api.Domain.Entities.Garage;

namespace Estapar.Garage.Api.Infrastructure.Persistence.Configurations;

public sealed class GarageConfiguration : IEntityTypeConfiguration<GarageEntity>
{
    public void Configure(EntityTypeBuilder<GarageEntity> builder)
    {
        builder.ToTable("Garages");

        builder.HasKey(g => g.Id);
        builder.Property(g => g.Name).HasMaxLength(100).IsRequired();
        builder.Property(g => g.CreatedAt);
        builder.Property(g => g.IsDeleted);

        builder.HasQueryFilter(g => !g.IsDeleted);

        builder.HasMany(g => g.Sectors)
            .WithOne(s => s.Garage)
            .HasForeignKey(s => s.GarageId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(g => g.Spots)
            .WithOne(s => s.Garage)
            .HasForeignKey(s => s.GarageId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasData(GarageSeedData.Garages());
    }
}
