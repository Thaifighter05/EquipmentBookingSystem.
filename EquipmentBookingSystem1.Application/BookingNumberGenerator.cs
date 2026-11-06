namespace EquipmentBookingSystem1.Application;

public class BookingNumberGenerator
{
    public Guid GenerateBookingNumber()
    {
        return Guid.NewGuid();
    }
}