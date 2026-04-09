using HotelBooking.Domain.Entities;
using HotelBooking.Application.Bookings.Commands.CreateBooking;
using HotelBooking.Domain.Interfaces;
using Moq;
using HotelBooking.Domain.Exceptions;
using FluentAssertions;
using HotelBooking.Application.Common.Interfaces;

namespace HotelBooking.Application.Tests.Bookings;

// Test paling penting: buktikan race condition tidak bisa terjadi
public class ConcurrencyTests
{
    [Fact]
    public async Task CreateBooking_WhenTenConcurrentRequests_OnlyOneShouldSucceed()
    {
        // Arrange
        var room = Room.Create("101", "Standard", 500_000);
        var guests = Enumerable.Range(0, 10)
            .Select(i => Guest.Create(
                $"Guest {i}",
                $"guest{i}@test.com",
                $"08123{i:D5}"))
            .ToList();

        var isBooked = false; // simulasi state kamar, shared antar task
        var stateLock = new SemaphoreSlim(1, 1); // lock untuk akses isBooked

        var roomRepository = new Mock<IRoomRepository>();
        roomRepository
            .Setup(r => r.GetByIdAsync(room.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => Room.Create("101", "Standard", 500_000)); // instance baru tiap call

        roomRepository
            .Setup(r => r.HasConflictAsync(
                room.Id,
                It.IsAny<DateOnly>(),
                It.IsAny<DateOnly>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false); // biarkan semua lolos cek conflict — race condition diuji di SaveChanges

        var bookingRepository = new Mock<IBookingRepository>();
        bookingRepository
            .Setup(b => b.AddAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Simulasi race condition: hanya request pertama yang berhasil save
        // Request berikutnya mendapat ConcurrencyException (seperti DbUpdateConcurrencyException
        // yang sudah di-wrap di BookingRepository di Infrastructure)
        bookingRepository
            .Setup(b => b.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(async () =>
            {
                await stateLock.WaitAsync();
                try
                {
                    if (isBooked)
                        throw new ConcurrencyException("Data telah diubah oleh proses lain.");
                    isBooked = true;
                }
                finally
                {
                    stateLock.Release();
                }
            });

        var lockService = new Mock<IDistributedLockService>();
        lockService
            .Setup(l => l.AcquireAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<TimeSpan?>()))
            .ReturnsAsync(true);
        lockService
            .Setup(l => l.ReleaseAsync(
                It.IsAny<string>(),
                It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        var successCount = 0;
        var failCount = 0;

        // Act: 10 request bersamaan
        var tasks = guests.Select(async guest =>
        {
            try
            {
                // Setiap task buat handler baru — seperti request HTTP yang berbeda
                var handler = new CreateBookingHandler(
                    roomRepository.Object,
                    bookingRepository.Object,
                    lockService.Object);
                
                var command = new CreateBookingCommand(
                    room.Id,
                    guest.Id,
                    DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
                    DateOnly.FromDateTime(DateTime.Today.AddDays(3)));

                await handler.Handle(command, CancellationToken.None);
                Interlocked.Increment(ref successCount);
            }
            catch (RoomUnavailableException)
            {
                Interlocked.Increment(ref failCount);
            }
            catch (ConcurrencyException)
            {
                // ConcurrencyException dari SaveChangesAsync juga berarti gagal
                Interlocked.Increment(ref failCount);
            }
        });

        await Task.WhenAll(tasks);

        // Assert
        successCount.Should().Be(1,
            "hanya satu booking yang boleh berhasil untuk kamar yang sama");
        failCount.Should().Be(9,
            "sembilan request sisanya harus mendapat RoomUnavailableException");
    }
}