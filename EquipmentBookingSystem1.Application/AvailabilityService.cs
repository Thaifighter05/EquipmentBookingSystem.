
using EquipmentBookingSystem1.Domain;

namespace EquipmentBookingSystem1.Application;

public class AvailabilityService
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IReadOnlyList<EquipmentItem> _equipmentItems;

    public AvailabilityService(
        IBookingRepository bookingRepository,
        IReadOnlyList<EquipmentItem> equipmentItems)
    {
        _bookingRepository = bookingRepository;
        _equipmentItems = equipmentItems;
    }

    public EquipmentItem? FindAvailableItem(
        Guid modelId,
        DateOnly startDate,
        DateOnly endDate)
    {
        // Steg 6: Hämta befintliga bokningar
        List<Booking> bookings =
            _bookingRepository.GetBookings();

        // Steg 7: Bokningslistan har returnerats

        // Steg 8: Kontrollera tillgängligheten
        EquipmentItem? availableItem = CheckAvailability(
            modelId,
            startDate,
            endDate,
            bookings);

        // Steg 9: Returnera ledigt exemplar eller null
        return availableItem;
    }

    private EquipmentItem? CheckAvailability(
        Guid modelId,
        DateOnly startDate,
        DateOnly endDate,
        List<Booking> bookings)
    {
        var requestedPeriod = new BookingPeriod(
            startDate,
            endDate);

        foreach (EquipmentItem item in _equipmentItems)
        {
            // Kontrollera bara exemplar av vald modell
            if (item.ModelId != modelId)
            {
                continue;
            }

            bool isBooked = bookings.Any(booking =>
                booking.EquipmentItemId == item.Id &&
                booking.Period.OverlapsWith(requestedPeriod));

            if (!isBooked)
            {
                return item;
            }
        }

        // Inget ledigt exemplar hittades
        return null;
    }
}
