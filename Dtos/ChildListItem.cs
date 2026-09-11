namespace OpenDaycare.Dtos;

public sealed record ChildListItem(
    Guid Id,
    string FullName,
    DateOnly BirthDate,
    Guid RoomId,
    string RoomName,
    string? MedicalNotes);
