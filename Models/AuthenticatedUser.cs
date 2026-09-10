namespace OpenDaycare.Models;

public sealed record AuthenticatedUser(
    Guid Id,
    string Email,
    string FullName,
    string Role,
    string? DaycareName,
    string Status);
