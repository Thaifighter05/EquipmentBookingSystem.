
using EquipmentBookingSystem1.Infrastructure;
using EquipmentBookingSystem1.Application;
using EquipmentBookingSystem1.Domain;
using Microsoft.Extensions.DependencyInjection;
using System.Globalization;

namespace EquipmentBookingSystem.Presentation;

public class ConsoleUI
{
    public static void Main(string[] args)
    {
        // Tillfällig kund eftersom vi inte har inloggning
        Guid customerId = Guid.NewGuid();

        // Två olika utrustningsmodeller
        Guid cameraModelId = Guid.NewGuid();
        Guid projectorModelId = Guid.NewGuid();

        // Två kameror och en projektor
        var equipmentItems = new List<EquipmentItem>
        {
            new EquipmentItem(Guid.NewGuid(), cameraModelId),
            new EquipmentItem(Guid.NewGuid(), cameraModelId),
            new EquipmentItem(Guid.NewGuid(), projectorModelId)
        };

        // TEST: Visa alla utrustningars unika exemplar-ID och modell-ID
        Console.WriteLine("=== TEST: Registrerad utrustning ===");

        foreach (EquipmentItem item in equipmentItems)
        {
            string modelName = item.ModelId == cameraModelId
                ? "Kamera"
                : "Projektor";

            Console.WriteLine(
                $"{modelName} | Exemplar-ID: {item.Id} | Modell-ID: {item.ModelId}");
        }

        Console.WriteLine();

        // Dependency Injection
        var services = new ServiceCollection();

        // Vanligt läge: Sparar bokningar i minnet.
        services.AddSingleton<IBookingRepository, InMemoryBookingRepository>();

        // TEST steg 16: Simulerar att sparandet misslyckas
        //services.AddSingleton<IBookingRepository, FailingBookingRepository>();

        services.AddSingleton<IReadOnlyList<EquipmentItem>>(equipmentItems);

        services.AddTransient<AvailabilityService>();
        services.AddTransient<BookingNumberGenerator>();
        services.AddTransient<BookingService>();

        using var serviceProvider = services.BuildServiceProvider();

        BookingService bookingService =
            serviceProvider.GetRequiredService<BookingService>();

        IBookingRepository bookingRepository =
            serviceProvider.GetRequiredService<IBookingRepository>();

        Console.WriteLine("Välkommen till bokningssystemet!");

        bool bookAnotherModel = true;

        while (bookAnotherModel)
        {
            Console.WriteLine();
            Console.WriteLine("Välj utrustningsmodell:");
            Console.WriteLine("1. Kamera (2 exemplar)");
            Console.WriteLine("2. Projektor (1 exemplar)");
            Console.Write("Ditt val: ");

            string? modelChoice = Console.ReadLine();

            Guid selectedModelId;
            string selectedModelName;

            if (modelChoice == "1")
            {
                selectedModelId = cameraModelId;
                selectedModelName = "Kamera";
            }
            else if (modelChoice == "2")
            {
                selectedModelId = projectorModelId;
                selectedModelName = "Projektor";
            }
            else
            {
                Console.WriteLine("Fel: Ogiltigt modellval.");
                continue;
            }

            Console.WriteLine();
            Console.WriteLine($"Boka utrustningsmodell: {selectedModelName}");

            // Steg 1: Kunden anger datum
            Console.Write("Startdatum (åååå-mm-dd): ");
            string? startInput = Console.ReadLine();

            Console.Write("Slutdatum (åååå-mm-dd): ");
            string? endInput = Console.ReadLine();

            bool validStartDate = DateOnly.TryParseExact(
                startInput,
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out DateOnly startDate);

            bool validEndDate = DateOnly.TryParseExact(
                endInput,
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out DateOnly endDate);

            if (!validStartDate || !validEndDate)
            {
                Console.WriteLine("Fel: Ange datum i formatet åååå-mm-dd.");
            }
            else
            {
                // Steg 2: Skicka bokningen till BookingService
                BookingResult result = bookingService.CreateBooking(
                    customerId,
                    selectedModelId,
                    startDate,
                    endDate);

                if (result.IsSuccess)
                {
                    // Steg 19: Visa bokningsbekräftelse
                    Console.WriteLine();
                    Console.WriteLine("Bokningen lyckades!");

                    Guid bookingNumber = result.Booking!.BookingNumber;

                    Console.WriteLine($"Bokningsnummer: {bookingNumber}");

                    // TEST: Visa vilket fysiskt exemplar som bokades
                    Console.WriteLine(
                        $"TEST: Bokat exemplar-ID: {result.Booking.EquipmentItemId}");

                    // Tillfällig kontroll för US-06:
                    // Finns samma bokningsnummer i repositoryt?
                    List<Booking> savedBookings =
                        bookingRepository.GetBookings();

                    Booking? savedBooking = savedBookings.FirstOrDefault(
                        booking => booking.BookingNumber == bookingNumber);

                    if (savedBooking != null)
                    {
                        Console.WriteLine(
                            $"TEST: Sparat bokningsnummer: {savedBooking.BookingNumber}");

                        Console.WriteLine(
                            "TEST: Bokningsnumret finns sparat.");

                        // TEST 4.4: Kontrollera uppgifterna i den sparade bokningen
                        Console.WriteLine(
                            $"TEST: Sparat exemplar-ID: {savedBooking.EquipmentItemId}");

                        Console.WriteLine(
                            $"TEST: Kund-ID vid bokning: {customerId}");

                        Console.WriteLine(
                            $"TEST: Sparat kund-ID: {savedBooking.CustomerId}");

                        Console.WriteLine(
                            $"TEST: Sparat startdatum: {savedBooking.Period.StartDate:yyyy-MM-dd}");

                        Console.WriteLine(
                            $"TEST: Sparat slutdatum: {savedBooking.Period.EndDate:yyyy-MM-dd}");
                    }
                    else
                    {
                        Console.WriteLine(
                            "TEST: Bokningen hittades inte i repositoryt.");
                    }
                }
                else
                {
                    // Steg 20: Visa felmeddelande
                    Console.WriteLine($"Fel: {result.ErrorMessage}");
                }
            }

            // Steg 21: Fråga om en ny bokning
            Console.WriteLine();
            Console.Write("Vill du boka en annan modell? (j/n): ");
            string? answer = Console.ReadLine();

            // Steg 22: Ta emot kundens svar
            bookAnotherModel =
                answer?.Trim().ToLowerInvariant() == "j";
        }

        Console.WriteLine();
        Console.WriteLine("Tack för att du använde bokningssystemet!");
    }
}
