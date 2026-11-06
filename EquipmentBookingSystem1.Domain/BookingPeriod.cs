
namespace EquipmentBookingSystem1.Domain;

public class BookingPeriod
{
    public DateOnly StartDate { get; }
    public DateOnly EndDate { get; }

    public BookingPeriod(DateOnly startDate, DateOnly endDate)
    {
        StartDate = startDate;
        EndDate = endDate;
    }

    public bool IsValid(DateOnly today)
    {
        return StartDate >= today &&
               EndDate >= StartDate;
    }

    public bool OverlapsWith(BookingPeriod other)
    {
        return StartDate <= other.EndDate &&
               EndDate >= other.StartDate;
    }
}
