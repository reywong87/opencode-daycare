namespace OpenDaycare.Dtos;

public sealed record ChildDetailItem(
    Guid Id,
    string FullName,
    DateOnly BirthDate,
    DateOnly EnrolledAt,
    string RoomName,
    string? MedicalNotes);
