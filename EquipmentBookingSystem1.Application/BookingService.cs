
using EquipmentBookingSystem1.Domain;


namespace EquipmentBookingSystem1.Application;

public class BookingService
{
    private readonly AvailabilityService _availabilityService;
    private readonly BookingNumberGenerator _bookingNumberGenerator;
    private readonly IBookingRepository _bookingRepository;

    public BookingService(
        AvailabilityService availabilityService,
        BookingNumberGenerator bookingNumberGenerator,
        IBookingRepository bookingRepository)
    {
        _availabilityService = availabilityService;
        _bookingNumberGenerator = bookingNumberGenerator;
        _bookingRepository = bookingRepository;
    }

    public BookingResult CreateBooking(
        Guid customerId,
        Guid modelId,
        DateOnly startDate,
        DateOnly endDate)
    {
        // Steg 3: Validera bokningsperioden
        bool isValid = ValidateBookingPeriod(startDate, endDate);

        // Steg 4: Ogiltig bokningsperiod
        if (!isValid)
        {
            return BookingResult.Failure("Ogiltig bokningsperiod");
        }

        // Steg 5: Hitta ett ledigt exemplar
        EquipmentItem? availableItem =
            _availabilityService.FindAvailableItem(
                modelId,
                startDate,
                endDate);

        // Steg 10: Inget ledigt exemplar hittades
        if (availableItem == null)
        {
            return BookingResult.Failure("Utrustningen är upptagen");
        }

        // Steg 11–12: Generera och ta emot bokningsnummer
        Guid bookingNumber =
            _bookingNumberGenerator.GenerateBookingNumber();

        // Steg 13: Skapa bokningen
        BookingPeriod period =
            new BookingPeriod(startDate, endDate);

        Booking booking = new Booking(
            bookingNumber,
            customerId,
            availableItem.Id,
            period);

        // Steg 14: Spara bokningen
        // Steg 15: Ta emot resultatet
        SaveResult saveResult =
            _bookingRepository.SaveBooking(booking);

        // Steg 16: Sparandet misslyckades
        if (!saveResult.IsSuccess)
        {
            return BookingResult.Failure(
                "Bokningen kunde inte genomföras");
        }

        // Steg 17: Sparandet lyckades
        // Steg 18: Returnera resultatet till ConsoleUI
        return BookingResult.Success(booking);
    }

    private bool ValidateBookingPeriod(
        DateOnly startDate,
        DateOnly endDate)
    {
        var period = new BookingPeriod(startDate, endDate);

        DateOnly today = DateOnly.FromDateTime(DateTime.Today);

        return period.IsValid(today);
    }
}
