using OpenDaycare.Dtos;

namespace OpenDaycare.Services;

public sealed class WallPostUiState
{
    public event Action? PostChanged;
    public event Action<PostFeedItem>? EditRequested;

    public void NotifyPostChanged() => PostChanged?.Invoke();

    public void RequestEdit(PostFeedItem post) => EditRequested?.Invoke(post);
}
