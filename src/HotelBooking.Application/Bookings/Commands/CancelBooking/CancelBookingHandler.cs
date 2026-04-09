using HotelBooking.Application.Common.Interfaces;
using HotelBooking.Domain.Entities;
using HotelBooking.Domain.Exceptions;
using MediatR;

namespace HotelBooking.Application.Bookings.Commands.CancelBooking;

public class CancelBookingHandler : IRequestHandler<CancelBookingCommand, CancelBookingResult>
{
    private readonly IBookingRepository _bookingRepository;

    public CancelBookingHandler(IBookingRepository bookingRepository) => _bookingRepository = bookingRepository;

    public async Task<CancelBookingResult> Handle(
        CancelBookingCommand cmd,
        CancellationToken ct)
    {
        var booking = await _bookingRepository.GetByIdAsync(cmd.BookingId, ct)
            ?? throw new NotFoundException(nameof(Booking), cmd.BookingId);
        
        booking.Cancel();

        await _bookingRepository.SaveChangesAsync(ct);

        return new CancelBookingResult(booking.Id, booking.Status == Domain.Enums.BookingStatus.Cancelled);
    }
}