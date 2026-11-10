namespace Zad.BuildingBlocks.Domain.Auditing;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class AuditLoggedAttribute : Attribute;
