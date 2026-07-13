using Estapar.ParkingManager.Application.DTO;
using Estapar.ParkingManager.Application.UseCases.Entry;
using Estapar.ParkingManager.Application.UseCases.Exit;
using Estapar.ParkingManager.Application.UseCases.Parked;
using Estapar.ParkingManager.Application.UseCases.Webhook;
using Estapar.ParkingManager.Domain.Entities;
using Estapar.ParkingManager.Tests.Application.Fakes;
using FluentAssertions;

namespace Estapar.ParkingManager.Tests.Application;

public class HandleWebhookEventUseCaseTests
{
    private static HandleWebhookEventUseCase CreateUseCase(out FakeParkingSessionRepository sessions)
    {
        var sectors = new FakeSectorRepository(Sector.Create(1, "A", 10m, 20));
        var spots = new FakeSpotRepository(Spot.Create(1, 1, -23.5, -46.6));
        sessions = new FakeParkingSessionRepository();
        var unitOfWork = new FakeUnitOfWork();

        var entryUseCase = new HandleEntryUseCase(sectors, sessions, unitOfWork);
        var parkedUseCase = new HandleParkedUseCase(sessions, spots, unitOfWork);
        var exitUseCase = new HandleExitUseCase(sessions, sectors, spots, unitOfWork);

        return new HandleWebhookEventUseCase(entryUseCase, parkedUseCase, exitUseCase);
    }

    [Fact]
    public async Task HandleAsync_EntryWithoutSector_ThrowsArgumentException()
    {
        var useCase = CreateUseCase(out _);
        var dto = new WebhookEventDto { EventType = EventType.ENTRY, LicensePlate = "ABC1234", EntryTime = DateTime.UtcNow };

        var act = () => useCase.HandleAsync(dto);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task HandleAsync_EntryWithoutEntryTime_ThrowsArgumentException()
    {
        var useCase = CreateUseCase(out _);
        var dto = new WebhookEventDto { EventType = EventType.ENTRY, LicensePlate = "ABC1234", SectorId = 1 };

        var act = () => useCase.HandleAsync(dto);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task HandleAsync_ParkedWithoutCoordinates_ThrowsArgumentException()
    {
        var useCase = CreateUseCase(out _);
        var dto = new WebhookEventDto { EventType = EventType.PARKED, LicensePlate = "ABC1234", SectorId = 1 };

        var act = () => useCase.HandleAsync(dto);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task HandleAsync_ExitWithoutExitTime_ThrowsArgumentException()
    {
        var useCase = CreateUseCase(out _);
        var dto = new WebhookEventDto { EventType = EventType.EXIT, LicensePlate = "ABC1234" };

        var act = () => useCase.HandleAsync(dto);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task HandleAsync_UnknownEventType_ThrowsArgumentException()
    {
        var useCase = CreateUseCase(out _);
        var dto = new WebhookEventDto { EventType = (EventType)99, LicensePlate = "ABC1234" };

        var act = () => useCase.HandleAsync(dto);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task HandleAsync_ValidEntry_DispatchesToHandleEntryUseCase()
    {
        var useCase = CreateUseCase(out var sessions);
        var dto = new WebhookEventDto
        {
            EventType = EventType.ENTRY,
            LicensePlate = "ABC1234",
            EntryTime = new DateTime(2026, 1, 1, 10, 0, 0, DateTimeKind.Utc),
            SectorId = 1
        };

        await useCase.HandleAsync(dto);

        sessions.Sessions.Should().Contain(s => s.SectorId == 1);
    }
}
