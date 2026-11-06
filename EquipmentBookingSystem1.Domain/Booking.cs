
namespace EquipmentBookingSystem1.Domain;

public class Booking
{
    public Guid BookingNumber { get; }
    public Guid CustomerId { get; }
    public Guid EquipmentItemId { get; }
    public BookingPeriod Period { get; }

    public Booking(
        Guid bookingNumber,
        Guid customerId,
        Guid equipmentItemId,
        BookingPeriod period)
    {
        BookingNumber = bookingNumber;
        CustomerId = customerId;
        EquipmentItemId = equipmentItemId;
        Period = period;
    }
}
