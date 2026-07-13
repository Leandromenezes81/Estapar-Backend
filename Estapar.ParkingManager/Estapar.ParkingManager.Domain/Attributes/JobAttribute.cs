namespace Estapar.ParkingManager.Domain.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class JobAttribute(Type jobGroup, string description = "") : Attribute
{
    public Type JobGroup { get; set; } = jobGroup;
    public string Description { get; private set; } = description;
}
