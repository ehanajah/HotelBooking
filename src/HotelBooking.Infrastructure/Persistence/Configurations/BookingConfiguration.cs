using HotelBooking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelBooking.Infrastructure.Persistence.Configurations;

public class BookingConfiguration: IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.HasKey(b => b.Id);

        // ── Properties ─────────────────────────────────────

        builder.Property(b => b.CheckIn)
            .IsRequired();

        builder.Property(b => b.CheckOut)
            .IsRequired();

        builder.Property(b => b.TotalPrice)
            .HasColumnType("decimal(10,2)");

        builder.Property(b => b.Status)
            .IsRequired()
            .HasConversion<string>() // enum → string (recommended)
            .HasMaxLength(10);

        builder.Property(b => b.CreatedAt)
            .IsRequired();

        // ── Relationships ─────────────────────────────────

        builder.HasOne(b => b.Room)
            .WithMany() // atau .WithMany(r => r.Bookings) jika ada collection
            .HasForeignKey(b => b.RoomId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(b => b.Guest)
            .WithMany()
            .HasForeignKey(b => b.GuestId)
            .OnDelete(DeleteBehavior.Restrict);

        // ── Indexes (PENTING untuk performa query kamu) ──

        builder.HasIndex(b => b.RoomId);

        builder.HasIndex(b => new { b.RoomId, b.CheckIn, b.CheckOut });

        builder.HasIndex(b => b.Status);
    }
}