using Estapar.Domain.Entities;
using Estapar.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Estapar.Infrastructure.Persistence.Configurations;

public sealed class ParkingSessionConfiguration : IEntityTypeConfiguration<ParkingSession>
{
    public void Configure(EntityTypeBuilder<ParkingSession> builder)
    {
        builder.ToTable("ParkingSessions");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.LicensePlate)
            .HasConversion(plate => plate.Value, value => LicensePlate.Create(value))
            .HasColumnName("LicensePlate")
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(p => p.SectorName).HasMaxLength(50).IsRequired();
        builder.Property(p => p.EntryTime).IsRequired();
        builder.Property(p => p.ExitTime);
        builder.Property(p => p.LockedPriceFactor).HasColumnType("decimal(5,2)");
        builder.Property(p => p.SpotId);

        builder.OwnsOne(p => p.AmountCharged, money =>
        {
            money.Property(m => m.Amount).HasColumnName("AmountCharged").HasColumnType("decimal(18,2)");
            money.Property(m => m.Currency).HasColumnName("AmountCurrency").HasMaxLength(3);
        });

        builder.HasOne<Sector>()
            .WithMany()
            .HasForeignKey(p => p.SectorName)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Spot>()
            .WithMany()
            .HasForeignKey(p => p.SpotId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(p => new { p.SectorName, p.ExitTime });
        builder.HasIndex(p => p.LicensePlate);
    }
}
