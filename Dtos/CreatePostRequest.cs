using Microsoft.AspNetCore.Components.Forms;

namespace OpenDaycare.Dtos;

public sealed record CreatePostRequest(
    string Type,
    string Body,
    IReadOnlyList<IBrowserFile> NewPhotos);
