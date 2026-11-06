using EquipmentBookingSystem1.Domain;

namespace EquipmentBookingSystem1.Application;

public interface IBookingRepository
{
    List<Booking> GetBookings();

    SaveResult SaveBooking(Booking booking);
}