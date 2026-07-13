namespace Estapar.ParkingManager.Domain.Attributes;

/// <summary>Marca uma classe como um job agendável, associando-a a um grupo e a uma descrição opcional.</summary>
/// <param name="jobGroup">Tipo usado para agrupar jobs relacionados.</param>
/// <param name="description">Descrição opcional do job.</param>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class JobAttribute(Type jobGroup, string description = "") : Attribute
{
    public Type JobGroup { get; set; } = jobGroup;
    public string Description { get; private set; } = description;
}
