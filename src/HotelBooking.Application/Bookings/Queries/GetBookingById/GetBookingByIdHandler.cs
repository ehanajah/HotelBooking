using HotelBooking.Application.Common.Interfaces;
using HotelBooking.Domain.Exceptions;
using MediatR;

namespace HotelBooking.Application.Bookings.Queries.GetBookingById;

public class GetBookingByIdHandler : IRequestHandler<GetBookingByIdQuery, BookingDto>
{
    private readonly IBookingRepository _bookingRepository;
    public GetBookingByIdHandler(IBookingRepository bookingRepository) => _bookingRepository = bookingRepository;
    public async Task<BookingDto> Handle(
        GetBookingByIdQuery query,
        CancellationToken ct)
    {
        var booking = await _bookingRepository.GetByIdAsync(query.BookingId, ct)
            ?? throw new NotFoundException(nameof(Domain.Entities.Booking), query.BookingId);

        return new BookingDto(
            booking.Id,
            booking.RoomId,
            booking.GuestId,
            booking.CheckIn,
            booking.CheckOut,
            booking.TotalPrice,
            booking.Room.RoomNumber,
            booking.Guest.Name);
    }
}