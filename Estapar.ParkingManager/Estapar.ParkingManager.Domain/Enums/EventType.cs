namespace Estapar.ParkingManager.Domain.Enums;

/// <summary>Tipo de evento recebido via webhook do simulador de estacionamento.</summary>
public enum EventType
{
    ENTRY = 0,
    PARKED = 1,
    EXIT = 2
}
