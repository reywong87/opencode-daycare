namespace OpenDaycare.Dtos;

public sealed record PostFeedItem(
    Guid Id,
    string AuthorName,
    Guid AuthorId,
    string Type,
    string Body,
    DateTimeOffset PublishedAt,
    IReadOnlyList<PostPhotoItem> Photos);
