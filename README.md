# EquipmentBookingSystem

## Om projektet

Detta projekt är en individuell uppgift i kursen Objektorienterad analys och design (OOAD).

Projektet är en förenklad bokningsapplikation utvecklad i C# och .NET. Syftet är att modellera och implementera ett sammanhängande bokningsflöde där en kund kan boka utrustning och få ett unikt bokningsnummer.

## Valda User Stories

**US-05 – Skapa bokning**

Som kund vill jag boka ett exemplar av en utrustningsmodell under en sammanhängande period så att utrustningen reserveras för mig.

**US-06 – Få bokningsnummer**

Som kund vill jag få ett bokningsnummer direkt när min bokning är genomförd så att jag kan använda det som referens vid kontakt med personalen.

## Funktionalitet

Den planerade implementationen omfattar:

- Kontroll av bokningsperiod och startdatum.
- Kontroll av tillgängliga utrustningsexemplar.
- Skapande av bokning och reservation av ett ledigt exemplar.
- Generering av ett unikt bokningsnummer.
- Visning av bokningsbekräftelse med bokningsnummer.
- Felmeddelande om bokningen inte kan genomföras.

## Avgränsning

Projektet är en förenklad implementation av det planerade bokningssystemet.

Det omfattar inte inloggning, grafiskt användargränssnitt, riktigt webb-API eller databas. Bokningarna hanteras i minnet under programmets körning.

Avancerad hantering av samtidiga bokningar, personalsökning och visning av tidigare bokningar ingår inte.

## UML-diagram

Funktionaliteten modelleras med:

- Ett aktivitetsdiagram som beskriver bokningsflödet och alternativa flöden.
- Ett sekvensdiagram som visar kommunikationen mellan systemets delar.

Diagrammen ligger till grund för C#-implementationen.

## Teknik

- C#
- .NET
- Visual Studio
- Konsolapplikation

## Köra projektet

1. Klona eller ladda ner projektet från GitHub.
2. Öppna `EquipmentBookingSystem.sln` i Visual Studio.
3. Starta konsolapplikationen.

Alternativt kan projektet köras med `dotnet run` från projektmappen om .NET SDK är installerat.

## Testning

Följande scenarier ska verifieras:

- En giltig bokning genomförs.
- En bokning med passerat startdatum avvisas.
- En bokning avvisas om utrustningen är upptagen.
- Två genomförda bokningar får olika bokningsnummer.

Testresultaten dokumenteras när implementationen är färdig.

## Status

Projektet är under utveckling som en del av den individuella OOAD-uppgiften.


## UML-diagram

Projektet bygger på US-05 och US-06 och beskriver
ett sammanhängande bokningsflöde.

### Aktivitetsdiagram

![Aktivitetsdiagram](Diagrams/Aktivitetsdiagram.png)

@startuml
title Aktivitetsdiagram - US-05 och US-06

skinparam monochrome true
skinparam shadowing false
skinparam backgroundColor white
skinparam defaultFontName Arial

skinparam activity {
    BackgroundColor white
    BorderColor black
    FontColor black
    DiamondBackgroundColor white
    DiamondBorderColor black
    ArrowColor black
    StartColor black
    EndColor black
}

start

repeat

    :Kunden bekräftar bokning\n(modell, startdatum, slutdatum);

    :Validera bokningsperiod\n(startdatum och slutdatum,\nbåda dagarna ingår);

    if (Är bokningsperioden giltig?) then (Ja)

        :Kontrollera tillgänglighet\nför vald modell och period\n(inklusive start- och slutdag);

        if (Finns ett ledigt exemplar?) then (Ja)

            :Välj ett ledigt exemplar;

            :Skapa bokning och generera\nunikt bokningsnummer;

            :Spara bokning med bokningsnummer\noch reservation tillsammans;

            if (Lyckades sparandet?) then (Ja)

                :Visa bokningsbekräftelse\nmed bokningsnummer;

            else (Nej)

                :Visa felmeddelande:\n"Bokningen kunde inte genomföras.";

            endif

        else (Nej)

            :Visa felmeddelande:\n"Utrustningen är upptagen\nunder perioden.";

        endif

    else (Nej)

        :Visa felmeddelande:\n"Ogiltig bokningsperiod.\nKontrollera start- och slutdatum.";

    endif

repeat while (Vill kunden boka ytterligare\nen utrustningsmodell?) is (Ja) not (Nej)

stop

caption Boka utrustning, reservera exemplar och få bokningsnummer
@enduml

### Sekvensdiagram

![Sekvensdiagram](Diagrams/SekvensDiagram.png)

@startuml
title Sekvensdiagram - US-05 och US-06

skinparam monochrome true
skinparam shadowing false
skinparam backgroundColor white
skinparam defaultFontName Arial
skinparam sequenceMessageAlign center
skinparam responseMessageBelowArrow true
autonumber 1

actor "Kund" as Customer
boundary "ConsoleUI" as UI
control "BookingService" as Service
control "AvailabilityService" as Availability
control "BookingNumberGenerator" as Generator
entity "BookingRepository" as Repository

loop För varje utrustningsmodell som kunden vill boka

    Customer -> UI : ConfirmBooking(modelId, startDate, endDate)
    UI -> Service : CreateBooking(modelId, startDate, endDate)

    Service -> Service : ValidateBookingPeriod(startDate, endDate)

# Implementation Plan – Equipment Booking System

## 1. Syfte

Syftet med implementationen är att testa om aktivitetsdiagrammet och sekvensdiagrammet för **US-05 och US-06** fungerar i praktiken genom att implementera bokningsflödet i C#.

Projektet byggs som en .NET Console App med objektorienterad programmering och en förenklad struktur inspirerad av Clean Architecture och Domain-Driven Design (DDD).

Diagrammen används som första designversion och kan behöva uppdateras efter implementation och testning.

## 2. User Stories

### US-05 – Boka utrustning

Som kund vill jag boka ett exemplar av en utrustningsmodell under en sammanhängande period så att utrustningen reserveras för mig.

Funktionalitet som ska testas:

- En giltig bokning reserverar ett tillgängligt exemplar.
- Både startdatum och slutdatum ingår i bokningsperioden.
- Startdatum i det förflutna godkänns inte.
- Bokning nekas när alla exemplar av modellen är upptagna under perioden.
- Bokning av flera utrustningsmodeller skapar separata bokningar.

### US-06 – Få bokningsnummer

Som kund vill jag få ett bokningsnummer direkt när min bokning är genomförd så att jag kan använda det som referens vid kontakt med personalen.

Funktionalitet som ska testas:

- Ett unikt bokningsnummer skapas vid en lyckad bokning.
- Bokningsnumret visas direkt i bekräftelsen.
- Olika bokningar får olika bokningsnummer.
- Bokningsnumret sparas tillsammans med bokningen och kan hämtas igen.

**Avgränsning:** Funktionen där kunden senare ser sina bokningar ingår inte i denna första implementation. Att numret sparas och kan hämtas testar en del av US-06 AC3, men uppfyller inte hela kriteriet.

## 3. Teknisk avgränsning

Den första implementationen ska använda:

- C# och .NET Console App.
- Objektorienterade klasser och metoder.
- Förenklad Clean Architecture.
- Grundläggande DDD-principer.
- Minneslagring med `List<Booking>`.
- Enkla tester av huvudflöde och alternativflöden.

Följande ingår inte:

- Web API.
- Grafiskt användargränssnitt.
- SQL-databas.
- Inloggningssystem.
- Bokningssökning för personal.
- Hantering av samtidiga bokningsförfrågningar.

Första versionen antar att kunden redan är inloggad. Detta är en avgränsning för testet, inte en färdig implementation av inloggning.

## 4. Planerade klasser och ansvar

### Domain

**Booking.cs**
- Representerar en bokning.
- Innehåller bokningsnummer, exemplar och bokningsperiod.
- Ska kunna skapas med giltiga värden.

**BookingPeriod.cs**
- Representerar startdatum och slutdatum.
- Kontrollerar att slutdatum inte är före startdatum.
- Kan kontrollera om två perioder överlappar.
- Båda gränsdagarna räknas med.

**EquipmentModel.cs**
- Representerar en utrustningsmodell.
- Innehåller exempelvis modell-ID och namn.

**EquipmentItem.cs**
- Representerar ett fysiskt exemplar.
- Innehåller exemplar-ID och modell-ID.

### Application

**BookingService.cs**
- Samordnar bokningsprocessen.
- Kontrollerar att startdatum inte ligger i det förflutna.
- Anropar tillgänglighetskontrollen.
- Begär ett bokningsnummer.
- Skapar och sparar bokningen.
- Returnerar ett resultat till användargränssnittet.

**AvailabilityService.cs**
- Söker efter ett ledigt exemplar av vald modell.
- Jämför önskad period med befintliga bokningar.
- Returnerar ett exemplar eller `null`.

**BookingResult.cs**
- Anger om bokningen lyckades.
- Innehåller bokningen vid lyckat resultat.
- Innehåller felmeddelande vid misslyckat resultat.

**IBookingRepository.cs**
- Definierar metoder för att spara och hämta bokningar.

**IBookingNumberGenerator.cs**
- Definierar en metod för att generera bokningsnummer.

### Infrastructure

**InMemoryBookingRepository.cs**
- Implementerar `IBookingRepository`.
- Sparar bokningar i en lista.
- Gör det möjligt att hämta sparade bokningar.
- Kan simulera ett misslyckat sparande för testning.

**GuidBookingNumberGenerator.cs**
- Implementerar `IBookingNumberGenerator`.
- Skapar bokningsnummer med `Guid.NewGuid()`.

### Presentation

**ConsoleUI.cs**
- Tar emot modell och bokningsdatum.
- Anropar `BookingService`.
- Visar bokningsbekräftelse eller felmeddelande.
- Frågar om kunden vill boka ytterligare en modell.

**Program.cs**
- Skapar och kopplar ihop objekten.
- Startar konsolprogrammet.
- Lägger in exempeldata med utrustningsmodeller och exemplar.

## 5. Planerade metoder

| Klass | Metod | Syfte |
|---|---|---|
| BookingService | `CreateBooking(...)` | Genomför bokningsflödet |
| BookingService | `ValidateBookingPeriod(...)` | Kontrollerar bokningsdatum |
| AvailabilityService | `FindAvailableItem(...)` | Hittar ett ledigt exemplar |
| BookingPeriod | `Overlaps(...)` | Kontrollerar överlappande perioder |
| IBookingNumberGenerator | `GenerateBookingNumber()` | Genererar bokningsnummer |
| IBookingRepository | `SaveBooking(...)` | Sparar bokningen |
| IBookingRepository | `GetBookings()` | Hämtar befintliga bokningar |
| IBookingRepository | `GetBookingByNumber(...)` | Hämtar bokning via nummer |
| ConsoleUI | `Run()` | Startar användarflödet |

Metodnamn och parametrar är preliminära och ska jämföras med sekvensdiagrammet under implementationen.

## 6. Bokningsflöde som ska implementeras

1. Kunden väljer en utrustningsmodell och anger startdatum och slutdatum.
2. `ConsoleUI` anropar `BookingService`.
3. `BookingService` validerar bokningsperioden.
4. Vid ogiltig period returneras ett felmeddelande.
5. Vid giltig period kontrollerar `AvailabilityService` om ett exemplar är ledigt.
6. Om inget exemplar är ledigt returneras ett felmeddelande.
7. Om ett exemplar finns genereras ett unikt bokningsnummer.
8. Ett `Booking`-objekt skapas.
9. Bokningen sparas i `InMemoryBookingRepository`.
10. Om sparandet misslyckas visas ett felmeddelande och ingen bokning sparas.
11. Om sparandet lyckas visas en bekräftelse med bokningsnumret.
12. Kunden kan välja att boka ytterligare en modell.

Varje lyckad bokning ska reservera exakt ett exemplar under vald period.

## 7. Tester

Följande scenarier ska testas:

- [ ] T01 – Giltig bokning skapas och sparas.
- [ ] T02 – Bokning med startdatum i det förflutna nekas.
- [ ] T03 – Bokning med slutdatum före startdatum nekas.
- [ ] T04 – Bokning där startdatum och slutdatum är samma dag godkänns.
- [ ] T05 – Överlappande bokning av ett redan reserverat exemplar nekas när inget annat exemplar finns.
- [ ] T06 – Bokning som börjar samma dag som en befintlig bokning slutar räknas som överlappande.
- [ ] T07 – Bokning av två olika modeller skapar två separata bokningar.
- [ ] T08 – Två lyckade bokningar får olika bokningsnummer.
- [ ] T09 – Bokningsnumret visas direkt efter lyckad bokning.
- [ ] T10 – Sparad bokning kan hämtas med samma bokningsnummer.
- [ ] T11 – Misslyckat sparande ger felmeddelande och ingen reservation skapas.
- [ ] T12 – Om en modell har flera exemplar kan ett annat ledigt exemplar bokas.

Testerna kan först köras manuellt via konsolen. Automatiserade tester kan läggas till om tiden räcker.

## 8. Implementationsordning

### Steg 1 – Domänmodellen
- [ ] Skapa mapparna Domain, Application, Infrastructure och Presentation.
- [ ] Implementera `EquipmentModel`.
- [ ] Implementera `EquipmentItem`.
- [ ] Implementera `BookingPeriod`.
- [ ] Implementera `Booking`.

### Steg 2 – Lagring och bokningsnummer
- [ ] Skapa `IBookingRepository`.
- [ ] Implementera `InMemoryBookingRepository`.
- [ ] Skapa `IBookingNumberGenerator`.
- [ ] Implementera `GuidBookingNumberGenerator`.

### Steg 3 – Bokningslogik
- [ ] Implementera `AvailabilityService`.
- [ ] Implementera `BookingResult`.
- [ ] Implementera `BookingService`.
- [ ] Säkerställ att ogiltiga bokningar inte sparas.

### Steg 4 – Konsolprogram
- [ ] Implementera `ConsoleUI`.
- [ ] Lägg till exempelutrustning i `Program.cs`.
- [ ] Koppla ihop klasserna.
- [ ] Genomför en bokning via konsolen.

### Steg 5 – Testning
- [ ] Kör samtliga testscenarier.
- [ ] Dokumentera förväntat och faktiskt resultat.
- [ ] Rätta eventuella fel.

### Steg 6 – UML och dokumentation
- [ ] Jämför aktivitetsdiagrammet med programflödet.
- [ ] Jämför sekvensdiagrammets klasser och metodanrop med koden.
- [ ] Uppdatera diagrammen om implementationen skiljer sig.
- [ ] Beskriv ändringar och motivera dem.
- [ ] Uppdatera README med körinstruktioner.
- [ ] Lägg till resultat från genomförda tester.

## 9. Koppling mellan UML och kod

Under implementationen ska följande kontrolleras:

- Finns motsvarande klasser för deltagarna i sekvensdiagrammet?
- Stämmer metodnamn och parametrar?
- Stämmer ordningen mellan anropen?
- Fungerar alla `alt`-flöden?
- Motsvarar konsolprogrammets beteende aktivitetsdiagrammet?
- Finns det skillnader som behöver förklaras eller uppdateras?

Målet är inte att den första diagramversionen ska vara perfekt, utan att slutversionen ska vara konsekvent med implementationen.

## 10. Status

**Status:** Planerad – implementation ännu inte påbörjad.

Planen är ett arbetsdokument och kan uppdateras när kod och tester utvecklas.
