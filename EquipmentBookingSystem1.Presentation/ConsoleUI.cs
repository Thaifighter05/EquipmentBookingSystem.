using Microsoft.Extensions.DependencyInjection;
using EquipmentBookingSystem1.Application;
using EquipmentBookingSystem1.Domain;
using EquipmentBookingSystem1.Infrastructure;


namespace EquipmentBookingSystem.Presentation;

public class ConsoleUI
{
    private readonly BookingService _bookingService;
    private readonly Guid _customerId;
    private readonly Guid _modelId;

    public ConsoleUI(
        BookingService bookingService,
        Guid customerId,
        Guid modelId)
    {
        _bookingService = bookingService;
        _customerId = customerId;
        _modelId = modelId;
    }

    public void Run()
    {
        Console.WriteLine("Välkommen till bokningssystemet!");

        bool bookAnotherModel = true;

        while (bookAnotherModel)
        {
            Console.WriteLine();
            Console.WriteLine("Boka utrustningsmodell: Testmodell");

            Console.Write("Startdatum (åååå-mm-dd): ");
            string? startInput = Console.ReadLine();

            Console.Write("Slutdatum (åååå-mm-dd): ");
            string? endInput = Console.ReadLine();

            if (!DateOnly.TryParse(startInput, out DateOnly startDate) ||
                !DateOnly.TryParse(endInput, out DateOnly endDate))
            {
                Console.WriteLine("Felaktigt datumformat.");
                continue;
            }

            // Steg 2: Skicka bokningsförfrågan
            BookingResult result = _bookingService.CreateBooking(
                _customerId,
                _modelId,
                startDate,
                endDate);

            // Steg 19–20: Visa resultat
            if (result.IsSuccess && result.Booking != null)
            {
                Console.WriteLine("Bokningen lyckades!");
                Console.WriteLine(
                    $"Bokningsnummer: {result.Booking.BookingNumber}");
            }
            else
            {
                Console.WriteLine(
                    $"Fel: {result.ErrorMessage}");
            }

            // Steg 21–22: Fråga om fler bokningar
            Console.Write("Vill du göra en ny bokning? (j/n): ");
            bookAnotherModel =
                Console.ReadLine()?.Trim().ToLower() == "j";
        }
    }

    public static void Main(string[] args)
    {
        var services = new ServiceCollection();

        // Testdata: En modell med ett fysiskt exemplar
        Guid modelId = Guid.NewGuid();
        Guid customerId = Guid.NewGuid();

        var equipmentItems = new List<EquipmentItem>
        {
            new EquipmentItem(Guid.NewGuid(), modelId)
        };

        // Registrera beroenden
        services.AddSingleton<IBookingRepository, InMemoryBookingRepository>();

        services.AddSingleton<IReadOnlyList<EquipmentItem>>(equipmentItems);

        services.AddTransient<AvailabilityService>();
        services.AddTransient<BookingNumberGenerator>();
        services.AddTransient<BookingService>();

        using var serviceProvider = services.BuildServiceProvider();

        BookingService bookingService =
            serviceProvider.GetRequiredService<BookingService>();

        var consoleUI = new ConsoleUI(
            bookingService,
            customerId,
            modelId);

        consoleUI.Run();
    }
}
