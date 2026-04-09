using HotelBooking.Application.Bookings.Commands.CancelBooking;
using HotelBooking.Application.Bookings.Commands.CreateBooking;
using HotelBooking.Application.Bookings.Queries.GetAvailableRooms;
using HotelBooking.Application.Bookings.Queries.GetBookingById;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace HotelBooking.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BookingsController : ControllerBase
{
    private readonly IMediator _mediator;

    public BookingsController(IMediator mediator) => _mediator = mediator;

    ///<summary>Buat booking baru</summary>
    [HttpPost]
    [ProducesResponseType(typeof(CreateBookingResult), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Create(
        [FromBody] CreateBookingCommand command,
        CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.BookingId },
            result);
    }

    ///<summary>Ambil detail booking</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(BookingDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetBookingByIdQuery(id), ct);
        return Ok(result);
    }

    ///<summary>Batalkan booking</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new CancelBookingCommand(id), ct);
        return NoContent();
    }
}

[ApiController]
[Route("api/[controller]")]
public class RoomsController : ControllerBase
{
    private readonly IMediator _mediator;

    public RoomsController(IMediator mediator) => _mediator = mediator;

    ///<summary>Cari kamar yang tersedia</summary>
    [HttpGet("available")]
    [ProducesResponseType(typeof(List<RoomDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAvailable(
        [FromQuery] DateOnly checkIn,
        [FromQuery] DateOnly checkOut,
        [FromQuery] string? roomType,
        CancellationToken ct)
    {
        var result = await _mediator.Send(
            new GetAvailableRoomsQuery(checkIn, checkOut, roomType), ct);

        return Ok(result);
    }
}