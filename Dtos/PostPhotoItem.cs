namespace OpenDaycare.Dtos;

public sealed record PostPhotoItem(
    Guid Id,
    string SignedUrl,
    int Position);
