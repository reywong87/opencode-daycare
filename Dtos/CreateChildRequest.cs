namespace OpenDaycare.Dtos;

public sealed record CreateChildRequest(
    string FullName,
    DateOnly BirthDate,
    Guid RoomId,
    string Allergies,
    string MedicalNotes);
