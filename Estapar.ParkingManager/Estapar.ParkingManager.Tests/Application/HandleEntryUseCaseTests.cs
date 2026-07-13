using Estapar.ParkingManager.Application.UseCases.Entry;
using Estapar.ParkingManager.Domain.Entities;
using Estapar.ParkingManager.Domain.Exceptions;
using Estapar.ParkingManager.Domain.ValueObjects;
using Estapar.ParkingManager.Tests.Application.Fakes;
using FluentAssertions;

namespace Estapar.ParkingManager.Tests.Application;

public class HandleEntryUseCaseTests
{
    private static readonly DateTime EntryTime = new(2026, 1, 1, 10, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task HandleAsync_UnknownSectorId_ThrowsInvalidOperationException()
    {
        var useCase = new HandleEntryUseCase(new FakeSectorRepository(), new FakeParkingSessionRepository(), new FakeUnitOfWork());
        var command = new EntryCommand("ABC1234", 999, EntryTime);

        var act = () => useCase.HandleAsync(command);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task HandleAsync_SectorAtFullCapacity_ThrowsGarageFullException()
    {
        var sector = Sector.Create(1, "A", 10m, maxCapacity: 1);
        var sectors = new FakeSectorRepository(sector);

        var openSession = ParkingSession.Open(LicensePlate.Create("XYZ0001"), sector.Id, EntryTime.AddHours(-1), 1.0m);
        var sessions = new FakeParkingSessionRepository(openSession);

        var useCase = new HandleEntryUseCase(sectors, sessions, new FakeUnitOfWork());
        var command = new EntryCommand("ABC1234", sector.Id, EntryTime);

        var act = () => useCase.HandleAsync(command);

        await act.Should().ThrowAsync<GarageFullException>();
    }

    [Fact]
    public async Task HandleAsync_SectorWithRoom_OpensSessionWithLockedPriceFactor()
    {
        var sector = Sector.Create(1, "A", 10m, maxCapacity: 20);
        var sectors = new FakeSectorRepository(sector);
        var sessions = new FakeParkingSessionRepository();
        var unitOfWork = new FakeUnitOfWork();
        var useCase = new HandleEntryUseCase(sectors, sessions, unitOfWork);

        var command = new EntryCommand("ABC1234", sector.Id, EntryTime);
        await useCase.HandleAsync(command);

        sessions.Sessions.Should().ContainSingle();
        var session = sessions.Sessions.Single();
        session.SectorId.Should().Be(sector.Id);
        session.LockedPriceFactor.Should().Be(0.90m); // 0 ocupadas / 20 vagas = 0% -> desconto
        unitOfWork.SaveChangesCallCount.Should().Be(1);
    }

    [Fact]
    public async Task HandleAsync_TwoSectorsWithSameNameDifferentGarages_AreIndependent()
    {
        // Regressão do bug corrigido nesta sessão: setores de garagens diferentes
        // podem ter o mesmo Name (ex. "A"), mas a lotação/fator de preço de um não
        // pode contaminar o cálculo do outro — só o SectorId específico importa.
        var sectorGarage1 = Sector.Create(1, "A", 10m, maxCapacity: 1);
        var sectorGarage2 = Sector.Create(5, "A", 10m, maxCapacity: 20);
        var sectors = new FakeSectorRepository(sectorGarage1, sectorGarage2);

        var sessionInGarage1 = ParkingSession.Open(LicensePlate.Create("XYZ0001"), sectorGarage1.Id, EntryTime.AddHours(-1), 1.0m);
        var sessions = new FakeParkingSessionRepository(sessionInGarage1);
        var unitOfWork = new FakeUnitOfWork();
        var useCase = new HandleEntryUseCase(sectors, sessions, unitOfWork);

        // sectorGarage1 já está lotado (1/1), mas a entrada é no sectorGarage2 (0/20) — deve funcionar.
        var command = new EntryCommand("ABC1234", sectorGarage2.Id, EntryTime);
        await useCase.HandleAsync(command);

        sessions.Sessions.Should().Contain(s => s.SectorId == sectorGarage2.Id);
    }
}
