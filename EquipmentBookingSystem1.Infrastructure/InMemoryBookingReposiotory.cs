using EquipmentBookingSystem1.Application;
using EquipmentBookingSystem1.Domain;

namespace EquipmentBookingSystem1.Infrastructure;

public class InMemoryBookingRepository : IBookingRepository
{
    private readonly List<Booking> _bookings = new();

    public List<Booking> GetBookings()
    {
        return _bookings.ToList();
    }

    public SaveResult SaveBooking(Booking booking)
    {
        _bookings.Add(booking);

        return SaveResult.Success();
    }
}