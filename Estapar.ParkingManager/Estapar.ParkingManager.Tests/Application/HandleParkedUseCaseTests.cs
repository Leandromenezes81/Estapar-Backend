using Estapar.ParkingManager.Application.UseCases.Parked;
using Estapar.ParkingManager.Domain.Entities;
using Estapar.ParkingManager.Domain.Exceptions;
using Estapar.ParkingManager.Domain.ValueObjects;
using Estapar.ParkingManager.Tests.Application.Fakes;
using FluentAssertions;

namespace Estapar.ParkingManager.Tests.Application;

public class HandleParkedUseCaseTests
{
    private static readonly DateTime EntryTime = new(2026, 1, 1, 10, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task HandleAsync_NoOpenSessionForPlate_ThrowsSessionNotFoundException()
    {
        var useCase = new HandleParkedUseCase(new FakeParkingSessionRepository(), new FakeSpotRepository(), new FakeUnitOfWork());
        var command = new ParkedCommand(1, "ABC1234", -23.5, -46.6);

        var act = () => useCase.HandleAsync(command);

        await act.Should().ThrowAsync<SessionNotFoundException>();
    }

    [Fact]
    public async Task HandleAsync_NoSpotAtCoordinatesInSector_ThrowsInvalidOperationException()
    {
        // A vaga existe, mas em outro setor — a busca é escopada por SectorId, não só lat/lng.
        var session = ParkingSession.Open(LicensePlate.Create("ABC1234"), 1, EntryTime, 1.0m);
        var sessions = new FakeParkingSessionRepository(session);
        var spotInOtherSector = Spot.Create(1, 5, -23.5, -46.6);

        var useCase = new HandleParkedUseCase(sessions, new FakeSpotRepository(spotInOtherSector), new FakeUnitOfWork());
        var command = new ParkedCommand(1, "ABC1234", -23.5, -46.6);

        var act = () => useCase.HandleAsync(command);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task HandleAsync_Success_OccupiesSpotAndAssignsToSession()
    {
        var session = ParkingSession.Open(LicensePlate.Create("ABC1234"), 1, EntryTime, 1.0m);
        var sessions = new FakeParkingSessionRepository(session);
        var spot = Spot.Create(1, 1, -23.5, -46.6);
        var spots = new FakeSpotRepository(spot);
        var unitOfWork = new FakeUnitOfWork();

        var useCase = new HandleParkedUseCase(sessions, spots, unitOfWork);
        var command = new ParkedCommand(1, "ABC1234", -23.5, -46.6);
        await useCase.HandleAsync(command);

        spot.IsAvailable.Should().BeFalse();
        session.SpotId.Should().Be(spot.Id);
        unitOfWork.SaveChangesCallCount.Should().Be(1);
    }
}
