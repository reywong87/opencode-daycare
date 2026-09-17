using Microsoft.AspNetCore.Components.Forms;

namespace OpenDaycare.Dtos;

public sealed record UpdatePostRequest(
    Guid Id,
    string Type,
    string Body,
    IReadOnlyList<Guid> RetainedPhotoIds,
    IReadOnlyList<IBrowserFile> NewPhotos);
