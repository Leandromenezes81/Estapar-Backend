using Estapar.ParkingManager.Domain.Entities;
using Estapar.ParkingManager.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Estapar.ParkingManager.Infrastructure.Data.Persistence.Configurations;

/// <summary>Mapeamento do EF Core para a entidade <see cref="ParkingSession"/>.</summary>
public sealed class ParkingSessionConfiguration : IEntityTypeConfiguration<ParkingSession>
{
    /// <summary>Configura a tabela, chave, propriedades (incluindo o objeto de valor <see cref="Money"/> embutido), relacionamento com <see cref="Spot"/> e índices de <see cref="ParkingSession"/>.</summary>
    public void Configure(EntityTypeBuilder<ParkingSession> builder)
    {
        builder.ToTable("ParkingSessions");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.LicensePlate)
            .HasConversion(plate => plate.Value, value => LicensePlate.Create(value))
            .HasColumnName("LicensePlate")
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(p => p.SectorId).IsRequired();
        builder.Property(p => p.EntryTime).IsRequired();
        builder.Property(p => p.ExitTime);
        builder.Property(p => p.LockedPriceFactor).HasColumnType("decimal(5,2)");
        builder.Property(p => p.SpotId);

        builder.OwnsOne(p => p.AmountCharged, money =>
        {
            money.Property(m => m.Amount).HasColumnName("AmountCharged").HasColumnType("decimal(18,2)");
            money.Property(m => m.Currency).HasColumnName("AmountCurrency").HasMaxLength(3);
        });

        builder.HasOne<Spot>()
            .WithMany()
            .HasForeignKey(p => p.SpotId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(p => new { p.SectorId, p.ExitTime });
        builder.HasIndex(p => new { p.LicensePlate, p.ExitTime });
    }
}
