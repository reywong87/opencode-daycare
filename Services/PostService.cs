using Microsoft.AspNetCore.Components.Forms;
using OpenDaycare.Dtos;
using OpenDaycare.Models;
using PostgrestConstants = Supabase.Postgrest.Constants;

namespace OpenDaycare.Services;

public sealed class PostService(
    Supabase.Client supabase,
    SupabaseAuthService authService,
    ILogger<PostService> logger)
{
    private const long MaxPhotoSize = 5 * 1024 * 1024;
    private const int MaxPhotoCount = 5;
    private static readonly TimeSpan SignedUrlLifetime = TimeSpan.FromMinutes(10);
    private static readonly IReadOnlySet<string> AllowedTypes = new HashSet<string>(StringComparer.Ordinal)
    {
        "meal", "nap", "activity", "achievement", "mood", "photo", "announcement"
    };
    private static readonly IReadOnlySet<string> AllowedContentTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg", "image/png", "image/webp"
    };

    public async Task<IReadOnlyList<PostFeedItem>> GetFeedAsync()
    {
        await authService.InitializeAsync();
        var profile = await GetActiveProfileAsync();

        try
        {
            var postsResponse = await supabase
                .From<Post>()
                .Filter("daycare_id", PostgrestConstants.Operator.Equals, profile.DaycareId!.Value.ToString())
                .Get();
            var photosResponse = await supabase.From<PostPhoto>().Get();
            var authorsResponse = await supabase
                .From<UserProfile>()
                .Filter("daycare_id", PostgrestConstants.Operator.Equals, profile.DaycareId.Value.ToString())
                .Filter("status", PostgrestConstants.Operator.Equals, "active")
                .Get();

            var authors = authorsResponse.Models.ToDictionary(author => author.Id);
            var photosByPost = photosResponse.Models
                .GroupBy(photo => photo.PostId)
                .ToDictionary(group => group.Key, group => group.OrderBy(photo => photo.Position).ToArray());

            var feed = new List<PostFeedItem>(postsResponse.Models.Count);
            foreach (var post in postsResponse.Models.OrderByDescending(post => post.PublishedAt))
            {
                if (!authors.TryGetValue(post.AuthorId, out var author))
                {
                    continue;
                }

                var photos = new List<PostPhotoItem>();
                if (photosByPost.TryGetValue(post.Id, out var postPhotos))
                {
                    foreach (var photo in postPhotos)
                    {
                        photos.Add(new PostPhotoItem(
                            photo.Id,
                            await supabase.Storage.From("post-images").CreateSignedUrl(
                                photo.StoragePath,
                                (int)SignedUrlLifetime.TotalSeconds),
                            photo.Position));
                    }
                }

                feed.Add(new PostFeedItem(
                    post.Id,
                    author.FullName,
                    post.AuthorId,
                    post.Type,
                    post.Body,
                    post.PublishedAt,
                    photos));
            }

            return feed;
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unable to load the authorized wall feed.");
            throw;
        }
    }

    public async Task CreateAsync(CreatePostRequest request)
    {
        await authService.InitializeAsync();
        ValidatePost(request.Type, request.Body, request.NewPhotos);
        var profile = await GetActiveProfileAsync(requireWriter: true);
        var uploadedPaths = new List<string>();
        Post? persistedPost = null;

        try
        {
            var post = new Post
            {
                DaycareId = profile.DaycareId!.Value,
                AuthorId = profile.Id,
                Type = request.Type,
                Body = request.Body.Trim()
            };
            var postResponse = await supabase.From<Post>().Insert(post);
            persistedPost = postResponse.Models.Single();
            logger.LogInformation("Wall post {PostId} is persisted before image upload.", persistedPost.Id);

            await UploadPhotosAsync(persistedPost, request.NewPhotos, uploadedPaths);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unable to create wall post for user {UserId}.", profile.Id);
            await CleanupFailedCreateAsync(uploadedPaths);
            if (persistedPost is not null)
            {
                try
                {
                    await supabase
                        .From<Post>()
                        .Filter("id", PostgrestConstants.Operator.Equals, persistedPost.Id.ToString())
                        .Delete();
                }
                catch (Exception cleanupException)
                {
                    logger.LogError(cleanupException, "Unable to remove the partially created wall post {PostId}.", persistedPost.Id);
                }
            }
            throw;
        }
    }

    public async Task UpdateAsync(UpdatePostRequest request)
    {
        await authService.InitializeAsync();
        ValidatePost(request.Type, request.Body, request.NewPhotos);
        var profile = await GetActiveProfileAsync(requireWriter: true);
        var uploadedPaths = new List<string>();

        try
        {
            var post = await GetOwnedPostAsync(request.Id, profile.Id);
            var existingPhotos = await GetPostPhotosAsync(post.Id);
            var retainedPhotoIds = request.RetainedPhotoIds.ToHashSet();
            if (retainedPhotoIds.Any(id => existingPhotos.All(photo => photo.Id != id)) ||
                retainedPhotoIds.Count + request.NewPhotos.Count > MaxPhotoCount)
            {
                throw new ArgumentException("La selección de imágenes no es válida.", nameof(request));
            }

            post.Type = request.Type;
            post.Body = request.Body.Trim();
            await supabase.From<Post>().Update(post);

            var removedPhotos = existingPhotos.Where(photo => !retainedPhotoIds.Contains(photo.Id)).ToArray();
            if (removedPhotos.Length > 0)
            {
                await supabase.Storage.From("post-images").Remove(removedPhotos.Select(photo => photo.StoragePath).ToList());
                foreach (var photo in removedPhotos)
                {
                    await supabase.From<PostPhoto>()
                        .Filter("id", PostgrestConstants.Operator.Equals, photo.Id.ToString())
                        .Delete();
                }
            }

            await UploadPhotosAsync(post, request.NewPhotos, uploadedPaths, retainedPhotoIds.Count);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unable to update wall post {PostId}.", request.Id);
            await CleanupFailedCreateAsync(uploadedPaths);
            throw;
        }
    }

    public async Task DeleteAsync(Guid postId)
    {
        await authService.InitializeAsync();
        var profile = await GetActiveProfileAsync(requireWriter: true);

        try
        {
            var post = await GetOwnedPostAsync(postId, profile.Id);
            var photos = await GetPostPhotosAsync(post.Id);
            if (photos.Count > 0)
            {
                await supabase.Storage.From("post-images").Remove(photos.Select(photo => photo.StoragePath).ToList());
            }

            await supabase
                .From<Post>()
                .Filter("id", PostgrestConstants.Operator.Equals, post.Id.ToString())
                .Delete();
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unable to delete wall post {PostId}.", postId);
            throw;
        }
    }

    private async Task<UserProfile> GetActiveProfileAsync(bool requireWriter = false)
    {
        var user = supabase.Auth.CurrentUser;
        if (user is null || !Guid.TryParse(user.Id, out var userId))
        {
            throw new InvalidOperationException("No hay una sesión autenticada.");
        }

        var response = await supabase
            .From<UserProfile>()
            .Filter("id", PostgrestConstants.Operator.Equals, userId.ToString())
            .Filter("status", PostgrestConstants.Operator.Equals, "active")
            .Get();
        var profile = response.Models.SingleOrDefault();
        if (profile?.DaycareId is null || (requireWriter && profile.Role is not ("staff" or "admin")))
        {
            throw new UnauthorizedAccessException("El perfil no está autorizado para este muro.");
        }

        return profile;
    }

    private async Task<Post> GetOwnedPostAsync(Guid postId, Guid authorId)
    {
        var response = await supabase
            .From<Post>()
            .Filter("id", PostgrestConstants.Operator.Equals, postId.ToString())
            .Filter("author_id", PostgrestConstants.Operator.Equals, authorId.ToString())
            .Get();
        return response.Models.SingleOrDefault()
            ?? throw new KeyNotFoundException("La publicación no existe o no está autorizada.");
    }

    private async Task<IReadOnlyList<PostPhoto>> GetPostPhotosAsync(Guid postId)
    {
        var response = await supabase
            .From<PostPhoto>()
            .Filter("post_id", PostgrestConstants.Operator.Equals, postId.ToString())
            .Get();
        return response.Models.OrderBy(photo => photo.Position).ToArray();
    }

    private async Task UploadPhotosAsync(
        Post post,
        IReadOnlyList<IBrowserFile> files,
        ICollection<string> uploadedPaths,
        int startingPosition = 0)
    {
        for (var index = 0; index < files.Count; index++)
        {
            var file = files[index];
            var photoId = Guid.NewGuid();
            var extension = GetStorageExtension(file.ContentType);
            var path = $"{post.DaycareId}/{post.Id}/{photoId}{extension}";
            logger.LogInformation(
                "Uploading wall post image at {StoragePath} with MIME type {ContentType}; authenticated storage session: {HasSession}.",
                path,
                file.ContentType,
                !string.IsNullOrWhiteSpace(supabase.Auth.CurrentSession?.AccessToken));
            await using var stream = file.OpenReadStream(MaxPhotoSize);
            await using var memory = new MemoryStream();
            await stream.CopyToAsync(memory);
            await supabase.Storage.From("post-images").Upload(memory.ToArray(), path, new Supabase.Storage.FileOptions
            {
                Upsert = false
            });
            uploadedPaths.Add(path);

            await supabase.From<PostPhoto>().Insert(new PostPhoto
            {
                Id = photoId,
                PostId = post.Id,
                StoragePath = path,
                Position = (short)(startingPosition + index)
            });
        }
    }

    private async Task CleanupFailedCreateAsync(IEnumerable<string> paths)
    {
        var pathsToRemove = paths.ToList();
        if (pathsToRemove.Count == 0)
        {
            return;
        }

        try
        {
            await supabase.Storage.From("post-images").Remove(pathsToRemove);
        }
        catch (Exception cleanupException)
        {
            logger.LogError(cleanupException, "Unable to clean up failed wall post uploads.");
        }
    }

    private static void ValidatePost(string type, string body, IReadOnlyList<IBrowserFile> photos)
    {
        if (!AllowedTypes.Contains(type))
        {
            throw new ArgumentException("El tipo de publicación no es válido.", nameof(type));
        }

        var trimmedBody = body?.Trim() ?? string.Empty;
        if (trimmedBody.Length is < 1 or > 1000)
        {
            throw new ArgumentException("El texto debe tener entre 1 y 1.000 caracteres.", nameof(body));
        }

        if (photos.Count > MaxPhotoCount)
        {
            throw new ArgumentException("No se pueden adjuntar más de cinco imágenes.", nameof(photos));
        }

        foreach (var photo in photos)
        {
            if (!AllowedContentTypes.Contains(photo.ContentType) || photo.Size > MaxPhotoSize)
            {
                throw new ArgumentException("Cada imagen debe ser JPG, PNG o WebP y pesar como máximo 5 MB.", nameof(photos));
            }
        }
    }

    private static string GetStorageExtension(string contentType) => contentType.ToLowerInvariant() switch
    {
        "image/jpeg" => ".jpg",
        "image/png" => ".png",
        "image/webp" => ".webp",
        _ => throw new ArgumentException("El formato de imagen no es válido.", nameof(contentType))
    };
}
