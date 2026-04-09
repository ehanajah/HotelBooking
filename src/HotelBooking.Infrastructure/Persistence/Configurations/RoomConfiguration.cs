using HotelBooking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelBooking.Infrastructure.Persistence.Configurations;

public class RoomConfiguration : IEntityTypeConfiguration<Room>
{
    public void Configure(EntityTypeBuilder<Room> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.RoomNumber)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(r => r.RoomType)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(r => r.PricePerNight)
            .HasColumnType("decimal(10,2)");

        // Konfigurasi RowVersion untuk optimistic concurrency
        builder.Property(r => r.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken();

        builder.HasIndex(r => r.RoomNumber).IsUnique();
    }
}