# Implementation Plan – Equipment Booking System

## Syfte och avgränsning

Implementera och testa **US-05 (boka utrustning)** och **US-06 (få bokningsnummer)** i en C#/.NET Console App. Använd en förenklad Clean Architecture med DDD-inspirerade domänobjekt. Detta är ett första test av UML-diagrammen: uppdatera diagrammen när faktisk kod skiljer sig.

- Ingen databas, API, GUI eller inloggning behövs. Kunden antas redan vara inloggad.
- Bokningar sparas bara så länge programmet körs.
- Varje bokning gäller **ett** fysiskt exemplar av **en** utrustningsmodell.
- Både start- och slutdatum räknas med.
- Senare visning av kundens bokningar (US-06 AC3) implementeras inte fullt ut; vi testar att samma nummer går att hämta från lagringen.
- Ingen parallell bokningshantering eller databastransaktion ingår.

## Föreslagen mappstruktur

```text
EquipmentBookingSystem/
├── README.md
├── IMPLEMENTATION_PLAN.md
├── Diagrams/
│   ├── ActivityDiagram.puml
│   ├── ActivityDiagram.png
│   ├── SequenceDiagram.puml
│   └── SequenceDiagram.png
└── EquipmentBookingSystem.ConsoleApp/
    ├── Domain/
    │   ├── EquipmentModel.cs
    │   ├── EquipmentItem.cs
    │   ├── BookingPeriod.cs
    │   └── Booking.cs
    ├── Application/
    │   ├── IBookingRepository.cs
    │   ├── IBookingNumberGenerator.cs
    │   ├── AvailabilityService.cs
    │   ├── BookingResult.cs
    │   └── BookingService.cs
    ├── Infrastructure/
    │   ├── InMemoryBookingRepository.cs
    │   └── GuidBookingNumberGenerator.cs
    ├── Presentation/
    │   └── ConsoleUI.cs
    └── Program.cs
```

Alla kodexempel nedan använder namnrymden `EquipmentBookingSystem.ConsoleApp` med respektive lager som suffix. Skapa en fil i taget och bygg projektet efter varje steg när dess beroenden finns på plats.

## Steg 1 – Skapa domänklasser

### 1.1 `Domain/EquipmentModel.cs`

En modell, exempelvis en viss kameramodell. Flera exemplar kan höra till samma modell.

```csharp
namespace EquipmentBookingSystem.ConsoleApp.Domain;

public record EquipmentModel(int Id, string Name);
```

### 1.2 `Domain/EquipmentItem.cs`

Ett fysiskt exemplar som kan reserveras.

```csharp
namespace EquipmentBookingSystem.ConsoleApp.Domain;

public record EquipmentItem(int Id, int ModelId);
```

### 1.3 `Domain/BookingPeriod.cs`

Ett DDD-inspirerat *Value Object* för perioden. Överlappning räknar med både första och sista dagen.

```csharp
namespace EquipmentBookingSystem.ConsoleApp.Domain;

public record BookingPeriod
{
    public DateOnly StartDate { get; }
    public DateOnly EndDate { get; }

    public BookingPeriod(DateOnly startDate, DateOnly endDate)
    {
        if (endDate < startDate)
            throw new ArgumentException("Slutdatum får inte vara före startdatum.");

        StartDate = startDate;
        EndDate = endDate;
    }

    public bool Overlaps(BookingPeriod other) =>
        StartDate <= other.EndDate && EndDate >= other.StartDate;
}
```

**Kontroll:** `2026-11-10`–`2026-11-12` överlappar `2026-11-12`–`2026-11-14`, eftersom den 12:e ingår i båda perioderna. En endagsbokning är giltig.

### 1.4 `Domain/Booking.cs`

Bokningen håller ihop bokningsnummer, valt exemplar och period. Det är bokningen som representerar reservationen; ingen separat `Reservation`-klass behövs i denna första version.

```csharp
namespace EquipmentBookingSystem.ConsoleApp.Domain;

public class Booking
{
    public string BookingNumber { get; }
    public int EquipmentItemId { get; }
    public BookingPeriod Period { get; }

    public Booking(string bookingNumber, int equipmentItemId, BookingPeriod period)
    {
        if (string.IsNullOrWhiteSpace(bookingNumber))
            throw new ArgumentException("Bokningsnummer krävs.");

        BookingNumber = bookingNumber;
        EquipmentItemId = equipmentItemId;
        Period = period;
    }
}
```

## Steg 2 – Definiera gränssnitt (Application)

### 2.1 `Application/IBookingRepository.cs`

`BookingService` ska bero på detta gränssnitt, inte på den konkreta listan i Infrastructure.

```csharp
using EquipmentBookingSystem.ConsoleApp.Domain;

namespace EquipmentBookingSystem.ConsoleApp.Application;

public interface IBookingRepository
{
    bool SaveBooking(Booking booking);
    IReadOnlyList<Booking> GetBookings();
    Booking? GetBookingByNumber(string bookingNumber);
}
```

### 2.2 `Application/IBookingNumberGenerator.cs`

```csharp
namespace EquipmentBookingSystem.ConsoleApp.Application;

public interface IBookingNumberGenerator
{
    string GenerateBookingNumber();
}
```

## Steg 3 – Implementera lagring och GUID

### 3.1 `Infrastructure/GuidBookingNumberGenerator.cs`

Här är den lilla kodbiten som skapar ett nytt GUID. Ett fullständigt GUID är praktiskt för testprojektet eftersom kollisioner är extremt osannolika. GUID ger dock inte en matematisk garanti för unikhet; repositoryt kontrollerar därför också dubbletter.

```csharp
using EquipmentBookingSystem.ConsoleApp.Application;

namespace EquipmentBookingSystem.ConsoleApp.Infrastructure;

public class GuidBookingNumberGenerator : IBookingNumberGenerator
{
    public string GenerateBookingNumber() => Guid.NewGuid().ToString("N");
}
```

**Exempel:** `f3a45c8b30d44c0e89a514b2145a1127`. Värdet ändras varje gång.

### 3.2 `Infrastructure/InMemoryBookingRepository.cs`

En lista ersätter databasen. `FailNextSave` låter oss testa felvägen i sekvensdiagrammet utan att behöva orsaka ett riktigt lagringsfel.

```csharp
using EquipmentBookingSystem.ConsoleApp.Application;
using EquipmentBookingSystem.ConsoleApp.Domain;

namespace EquipmentBookingSystem.ConsoleApp.Infrastructure;

public class InMemoryBookingRepository : IBookingRepository
{
    private readonly List<Booking> _bookings = new();
    public bool FailNextSave { get; set; }

    public bool SaveBooking(Booking booking)
    {
        if (FailNextSave)
        {
            FailNextSave = false;
            return false;
        }

        if (_bookings.Any(b => b.BookingNumber == booking.BookingNumber))
            return false;

        // Skydd mot överlappning på samma fysiska exemplar.
        if (_bookings.Any(b => b.EquipmentItemId == booking.EquipmentItemId &&
                               b.Period.Overlaps(booking.Period)))
            return false;

        _bookings.Add(booking);
        return true;
    }

    public IReadOnlyList<Booking> GetBookings() => _bookings.AsReadOnly();

    public Booking? GetBookingByNumber(string bookingNumber) =>
        _bookings.FirstOrDefault(b => b.BookingNumber == bookingNumber);
}
```

**Kontroll:** Lagringen ska inte lägga till något när `SaveBooking` returnerar `false`.

## Steg 4 – Implementera tillgänglighet

### `Application/AvailabilityService.cs`

Den här tjänsten behöver känna till vilka fysiska exemplar som finns och vilka som redan har bokats.

```csharp
using EquipmentBookingSystem.ConsoleApp.Domain;

namespace EquipmentBookingSystem.ConsoleApp.Application;

public class AvailabilityService
{
    private readonly IReadOnlyList<EquipmentItem> _items;
    private readonly IBookingRepository _repository;

    public AvailabilityService(
        IReadOnlyList<EquipmentItem> items,
        IBookingRepository repository)
    {
        _items = items;
        _repository = repository;
    }

    public EquipmentItem? FindAvailableItem(int modelId, BookingPeriod period)
    {
        foreach (var item in _items.Where(i => i.ModelId == modelId))
        {
            bool isBooked = _repository.GetBookings().Any(b =>
                b.EquipmentItemId == item.Id && b.Period.Overlaps(period));

            if (!isBooked)
                return item;
        }

        return null; 
    }
}
```

**Kontroll:** Finns två exemplar av samma modell kan det andra väljas när det första är upptaget.

## Steg 5 – Resultat och bokningsprocess

### 5.1 `Application/BookingResult.cs`

Resultatet är antingen en lyckad bokning eller ett felmeddelande.

```csharp
using EquipmentBookingSystem.ConsoleApp.Domain;

namespace EquipmentBookingSystem.ConsoleApp.Application;

public record BookingResult(bool IsSuccess, Booking? Booking, string? ErrorMessage)
{
    public static BookingResult Success(Booking booking) =>
        new(true, booking, null);

    public static BookingResult Failure(string message) =>
        new(false, null, message);
}
```

### 5.2 `Application/BookingService.cs`

Denna klass samordnar sekvensdiagrammets huvudflöde och `alt`-grenar.

```csharp
using EquipmentBookingSystem.ConsoleApp.Domain;

namespace EquipmentBookingSystem.ConsoleApp.Application;

public class BookingService
{
    private readonly AvailabilityService _availability;
    private readonly IBookingRepository _repository;
    private readonly IBookingNumberGenerator _numberGenerator;

    public BookingService(
        AvailabilityService availability,
        IBookingRepository repository,
        IBookingNumberGenerator numberGenerator)
    {
        _availability = availability;
        _repository = repository;
        _numberGenerator = numberGenerator;
    }

    public BookingResult CreateBooking(
        int modelId, DateOnly startDate, DateOnly endDate)
    {
        if (startDate < DateOnly.FromDateTime(DateTime.Today) || endDate < startDate)
            return BookingResult.Failure("Ogiltig bokningsperiod.");

        var period = new BookingPeriod(startDate, endDate);
        var item = _availability.FindAvailableItem(modelId, period);

        if (item is null)
            return BookingResult.Failure("Utrustningen är upptagen eller saknas.");

        string bookingNumber = _numberGenerator.GenerateBookingNumber();
        var booking = new Booking(bookingNumber, item.Id, period);

        if (!_repository.SaveBooking(booking))
            return BookingResult.Failure("Bokningen kunde inte genomföras.");

        return BookingResult.Success(booking);
    }
}
```

**Viktigt:** Vid misslyckat sparande returneras inget bokningsnummer till kunden och ingen reservation läggs till. Ett genererat GUID som inte används är inget problem.

## Steg 6 – Konsolgränssnitt

### `Presentation/ConsoleUI.cs`

Här används `DateOnly.TryParse` så att felaktig inmatning inte kraschar programmet. Använd formatet `yyyy-MM-dd` i testerna.

```csharp
using EquipmentBookingSystem.ConsoleApp.Application;

namespace EquipmentBookingSystem.ConsoleApp.Presentation;

public class ConsoleUI
{
    private readonly BookingService _bookingService;

    public ConsoleUI(BookingService bookingService)
    {
        _bookingService = bookingService;
    }

    public void Run()
    {
        bool bookAnotherModel;

        do
        {
            Console.Write("Modell-ID: ");
            if (!int.TryParse(Console.ReadLine(), out int modelId))
            {
                Console.WriteLine("Ogiltigt modell-ID.");
                bookAnotherModel = AskToBookAnotherModel();
                continue;
            }

            Console.Write("Startdatum (yyyy-MM-dd): ");
            bool validStart = DateOnly.TryParse(Console.ReadLine(), out var startDate);

            Console.Write("Slutdatum (yyyy-MM-dd): ");
            bool validEnd = DateOnly.TryParse(Console.ReadLine(), out var endDate);

            if (!validStart || !validEnd)
            {
                Console.WriteLine("Ogiltigt datumformat.");
            }
            else
            {
                var result = _bookingService.CreateBooking(modelId, startDate, endDate);
                if (result.IsSuccess)
                    Console.WriteLine($"Bokningen lyckades! Bokningsnummer: {result.Booking!.BookingNumber}");
                else
                    Console.WriteLine($"Fel: {result.ErrorMessage}");
            }

            bookAnotherModel = AskToBookAnotherModel();
        }
        while (bookAnotherModel);
    }

    private static bool AskToBookAnotherModel()
    {
        Console.Write("Boka ytterligare en modell? (j/n): ");
        return Console.ReadLine()?.Trim().ToLowerInvariant() == "j";
    }
}
```

## Steg 7 – Koppla ihop allt

### `Program.cs`

Detta är en enkel manuell sammansättning av beroendena (*dependency injection* utan extra bibliotek).

```csharp
using EquipmentBookingSystem.ConsoleApp.Application;
using EquipmentBookingSystem.ConsoleApp.Domain;
using EquipmentBookingSystem.ConsoleApp.Infrastructure;
using EquipmentBookingSystem.ConsoleApp.Presentation;

var models = new List<EquipmentModel>
{
    new(1, "Kamera"),
    new(2, "Projektor")
};

var items = new List<EquipmentItem>
{
    new(101, 1),
    new(102, 1),
    new(201, 2)
};

var repository = new InMemoryBookingRepository();
var numberGenerator = new GuidBookingNumberGenerator();
var availability = new AvailabilityService(items, repository);
var bookingService = new BookingService(availability, repository, numberGenerator);
var ui = new ConsoleUI(bookingService);

Console.WriteLine("Tillgängliga modeller:");
foreach (var model in models)
    Console.WriteLine($"{model.Id}: {model.Name}");

ui.Run();
```

**Kontroll:** Starta programmet med `dotnet run` från projektmappen, eller med Start i Visual Studio. Gör en bokning med dagens eller ett framtida datum. Bekräftelsen ska visa ett GUID.

## Steg 8 – Testa ett scenario i taget

Använd framtida datum när du kör testerna. Kontrollera både konsolutskrift och lagrade bokningar. Kryssa först efter att testet faktiskt genomförts.

- [ ] **T01 Lyckad bokning:** Modell 2, ledig period → bokning och nummer visas.
- [ ] **T02 Datum i det förflutna:** Startdatum före dagens datum → fel, ingen bokning.
- [ ] **T03 Omvänd period:** Slutdatum före startdatum → fel.
- [ ] **T04 Endagsbokning:** Samma start- och slutdatum → lyckas.
- [ ] **T05 Upptagen modell:** Boka modell 2 två gånger med överlappande period → andra försöket nekas.
- [ ] **T06 Inkluderande slutdag:** Boka modell 2 till den 12:e; ny bokning från den 12:e → nekas.
- [ ] **T07 Två modeller:** Boka modell 1 och 2 → två separata bokningar.
- [ ] **T08 Olika nummer:** Två lyckade bokningar → olika GUID.
- [ ] **T09 Nummer i bekräftelse:** Kontrollera att numret skrivs ut direkt.
- [ ] **T10 Samma nummer vid hämtning:** Anropa `repository.GetBookingByNumber(number)` i ett test och kontrollera att rätt bokning returneras.
- [ ] **T11 Misslyckat sparande:** Sätt `repository.FailNextSave = true` före en bokning; kontrollera fel och att antalet sparade bokningar inte ökar.
- [ ] **T12 Flera exemplar:** Modell 1 har två exemplar; två överlappande bokningar ska kunna lyckas på olika exemplar, en tredje ska nekas.

**Observera:** T10 och T11 kräver ett litet separat test eller tillfällig testkod; de är inte exponerade som menyval i konsolen. Rensa eller starta om minneslagringen mellan testscenarier när det behövs. Datumexemplen måste alltid ligga i framtiden relativt testdagen.

### Enkel testkod för T10

Efter en lyckad bokning kan följande användas i ett separat test:

```csharp
var saved = repository.GetBookingByNumber(result.Booking!.BookingNumber);
Console.WriteLine(saved?.BookingNumber == result.Booking.BookingNumber);
// Förväntat: True
```

Variabeln `result` måste då komma från ett anrop till `bookingService.CreateBooking(...)` i samma testkod.

### Enkel testkod för T11

```csharp
int before = repository.GetBookings().Count;
repository.FailNextSave = true;
var failed = bookingService.CreateBooking(2, startDate, endDate);
Console.WriteLine(!failed.IsSuccess);
Console.WriteLine(repository.GetBookings().Count == before);
// Förväntat: True och True
```

Använd en giltig, ledig period för T11; annars stoppas bokningen innan sparandet testas.

## Steg 9 – Jämför UML och kod

- [ ] `ConsoleUI` anropar `BookingService.CreateBooking(...)`.
- [ ] Datum valideras före tillgänglighetskontrollen.
- [ ] `AvailabilityService` söker exemplar och kontrollerar överlappning.
- [ ] `IBookingNumberGenerator` används för bokningsnummer.
- [ ] `IBookingRepository.SaveBooking(...)` används för att spara.
- [ ] Felvägarna i sekvensdiagrammets `alt`-block finns i koden.
- [ ] Upprepningen i aktivitetsdiagrammet finns i konsolens loop.
- [ ] Justera sekvensdiagrammet: `BookingPeriod` har tillkommit; `AvailabilityService` läser från repository; repository är ett gränssnitt; `SaveBooking` returnerar `bool` i stället för `SaveResult`.
- [ ] Dokumentera skillnaderna mellan UML version 1 och slutlig kod.

## Steg 10 – Dokumentera och lämna in

- [ ] Spara redigerbara `.puml`-filer och bilder i `Diagrams/`.
- [ ] Lägg in UML-bilder och länk till denna plan i `README.md`.
- [ ] Beskriv hur projektet körs.
- [ ] Dokumentera vilka tester som genomfördes och resultaten.
- [ ] Förklara ansvarsfördelning och hur UML motsvarar koden.
- [ ] Kontrollera med läraren hur den avgränsade hanteringen av US-06 AC3 ska redovisas.
- [ ] Commit och push till GitHub.

## Länk från README.md

Lägg till följande i README:

```markdown
## Implementationsplan

[Se steg-för-steg-planen med C#-kod](IMPLEMENTATION_PLAN.md)
```

**Status:** Plan och kodexempel. Inga tester är markerade som genomförda.