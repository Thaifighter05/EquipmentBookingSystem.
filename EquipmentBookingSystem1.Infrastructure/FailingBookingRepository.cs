using EquipmentBookingSystem1.Application;
using EquipmentBookingSystem1.Domain;

namespace EquipmentBookingSystem.Infrastructure;

public class FailingBookingRepository : IBookingRepository
{
    public List<Booking> GetBookings()
    {
        return new List<Booking>();
    }

    public SaveResult SaveBooking(Booking booking)
    {
        // Simulera ett misslyckat sparande
        return SaveResult.Failure();
    }
}
