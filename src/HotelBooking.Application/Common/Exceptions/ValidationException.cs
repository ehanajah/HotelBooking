using FluentValidation.Results;

namespace HotelBooking.Application.Common.Exceptions;

public class ValidationException : Exception
{
    public IDictionary<string, string[]> Errors { get; }

    public ValidationException(IEnumerable<ValidationFailure> failures)
        : base("One or more validation failures have occured.")
    {
        // Kelompokkan error berdasarkan nama property
        // contoh: { "CheckIn": ["Tidak boleh di masa lalu."], "RoomId": ["Tidak boleh kosong."] }
        Errors = failures
            .GroupBy(f => f.PropertyName, f => f.ErrorMessage)
            .ToDictionary(
                group => group.Key,
                group => group.ToArray()
            );
    }
}