
using EquipmentBookingSystem1.Domain;

namespace EquipmentBookingSystem1.Application;

public class BookingResult
{
    public bool IsSuccess { get; }
    public Booking? Booking { get; }
    public string? ErrorMessage { get; }

    private BookingResult(
        bool isSuccess,
        Booking? booking,
        string? errorMessage)
    {
        IsSuccess = isSuccess;
        Booking = booking;
        ErrorMessage = errorMessage;
    }

    public static BookingResult Success(Booking booking)
    {
        return new BookingResult(true, booking, null);
    }

    public static BookingResult Failure(string errorMessage)
    {
        return new BookingResult(false, null, errorMessage);
    }
}
