using Estapar.ParkingManager.Application.UseCases.Exit;
using Estapar.ParkingManager.Domain.Entities;
using Estapar.ParkingManager.Domain.Exceptions;
using Estapar.ParkingManager.Domain.ValueObjects;
using Estapar.ParkingManager.Tests.Application.Fakes;
using FluentAssertions;

namespace Estapar.ParkingManager.Tests.Application;

public class HandleExitUseCaseTests
{
    private static readonly DateTime EntryTime = new(2026, 1, 1, 10, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime ExitTime = EntryTime.AddHours(2);

    [Fact]
    public async Task HandleAsync_NoOpenSessionForPlate_ThrowsSessionNotFoundException()
    {
        var useCase = new HandleExitUseCase(
            new FakeParkingSessionRepository(),
            new FakeSectorRepository(),
            new FakeSpotRepository(),
            new FakeUnitOfWork());

        var command = new ExitCommand("ABC1234", ExitTime);

        var act = () => useCase.HandleAsync(command);

        await act.Should().ThrowAsync<SessionNotFoundException>();
    }

    [Fact]
    public async Task HandleAsync_SectorNoLongerExists_ThrowsInvalidOperationException()
    {
        var session = ParkingSession.Open(LicensePlate.Create("ABC1234"), 1, EntryTime, 1.0m);
        var sessions = new FakeParkingSessionRepository(session);

        var useCase = new HandleExitUseCase(sessions, new FakeSectorRepository(), new FakeSpotRepository(), new FakeUnitOfWork());
        var command = new ExitCommand("ABC1234", ExitTime);

        var act = () => useCase.HandleAsync(command);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task HandleAsync_Success_ChargesFeeAndReleasesSpot()
    {
        var sector = Sector.Create(1, "A", 10m, maxCapacity: 20);
        var spot = Spot.Create(1, sector.Id, -23.5, -46.6);
        spot.Occupy();

        var session = ParkingSession.Open(LicensePlate.Create("ABC1234"), sector.Id, EntryTime, 1.0m);
        session.AssignSpot(spot.Id);

        var sessions = new FakeParkingSessionRepository(session);
        var spots = new FakeSpotRepository(spot);
        var unitOfWork = new FakeUnitOfWork();
        var useCase = new HandleExitUseCase(sessions, new FakeSectorRepository(sector), spots, unitOfWork);

        var command = new ExitCommand("ABC1234", ExitTime);
        await useCase.HandleAsync(command);

        session.IsOpen.Should().BeFalse();
        session.AmountCharged!.Amount.Should().Be(20m); // 2h * 10 base * fator 1.0
        spot.IsAvailable.Should().BeTrue();
        unitOfWork.SaveChangesCallCount.Should().Be(1);
    }
}
