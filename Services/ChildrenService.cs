using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using OpenDaycare.Dtos;
using OpenDaycare.Models;
using PostgrestConstants = Supabase.Postgrest.Constants;

namespace OpenDaycare.Services;

public sealed class ChildrenService(
    Supabase.Client supabase,
    SupabaseAuthService authService,
    ILogger<ChildrenService> logger)
{
    public async Task<IReadOnlyList<Room>> GetRoomsAsync()
    {
        await authService.InitializeAsync();

        try
        {
            var response = await supabase.From<Room>().Get();
            return response.Models
                .OrderBy(room => room.Name, StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unable to load authorized rooms.");
            throw;
        }
    }

    public async Task<IReadOnlyList<ChildListItem>> GetActiveChildrenAsync()
    {
        await authService.InitializeAsync();

        try
        {
            var roomsResponse = await supabase.From<Room>().Get();
            var childrenResponse = await supabase
                .From<Child>()
                .Filter("status", PostgrestConstants.Operator.Equals, "active")
                .Get();
            var roomsById = roomsResponse.Models.ToDictionary(room => room.Id);

            return childrenResponse.Models
                .Where(child => roomsById.TryGetValue(child.RoomId, out _))
                .Select(child => new ChildListItem(
                    child.Id,
                    child.Slug,
                    child.FullName,
                    child.BirthDate,
                    child.RoomId,
                    roomsById[child.RoomId].Name,
                    child.MedicalNotes))
                .OrderBy(child => child.RoomName, StringComparer.OrdinalIgnoreCase)
                .ThenBy(child => child.FullName, StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unable to load active children.");
            throw;
        }
    }

    public async Task CreateChildAsync(CreateChildRequest request)
    {
        await authService.InitializeAsync();

        var slug = await CreateUniqueSlugAsync(request.FullName);
        var child = new Child
        {
            RoomId = request.RoomId,
            FullName = request.FullName.Trim(),
            Slug = slug,
            BirthDate = request.BirthDate,
            EnrolledAt = DateOnly.FromDateTime(DateTime.Today),
            MedicalNotes = FormatMedicalNotes(request.Allergies, request.MedicalNotes),
            PhotoConsent = true,
            Status = "active"
        };

        try
        {
            await supabase.From<Child>().Insert(child);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unable to create a child.");
            throw;
        }
    }

    public async Task<ChildDetailItem?> GetActiveChildBySlugAsync(string slug)
    {
        await authService.InitializeAsync();

        try
        {
            var childResponse = await supabase
                .From<Child>()
                .Filter("slug", PostgrestConstants.Operator.Equals, slug)
                .Filter("status", PostgrestConstants.Operator.Equals, "active")
                .Get();
            var child = childResponse.Models.SingleOrDefault();
            if (child is null)
            {
                return null;
            }

            var roomResponse = await supabase
                .From<Room>()
                .Filter("id", PostgrestConstants.Operator.Equals, child.RoomId.ToString())
                .Get();
            var room = roomResponse.Models.SingleOrDefault();
            return room is null
                ? null
                : new ChildDetailItem(
                    child.Id,
                    child.FullName,
                    child.BirthDate,
                    child.EnrolledAt,
                    room.Name,
                    child.MedicalNotes);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unable to load the active child with slug {Slug}.", slug);
            throw;
        }
    }

    private async Task<string> CreateUniqueSlugAsync(string fullName)
    {
        var existingSlugs = (await supabase.From<Child>().Get()).Models
            .Select(child => child.Slug)
            .Where(slug => !string.IsNullOrWhiteSpace(slug))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var baseSlug = NormalizeSlug(fullName);
        var candidateSlug = baseSlug;
        var suffix = 2;

        while (!existingSlugs.Add(candidateSlug))
        {
            candidateSlug = $"{baseSlug}-{suffix}";
            suffix++;
        }

        return candidateSlug;
    }

    private static string NormalizeSlug(string fullName)
    {
        var words = Regex.Split(fullName.Trim(), @"\s+")
            .Where(word => !string.IsNullOrWhiteSpace(word))
            .Take(3);
        var source = string.Join('-', words).Normalize(NormalizationForm.FormD);
        var withoutAccents = new string(source
            .Where(character => CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark)
            .ToArray());
        var slug = Regex.Replace(withoutAccents.ToLowerInvariant(), "[^a-z0-9]+", "-").Trim('-');
        return string.IsNullOrWhiteSpace(slug) ? "nino" : slug;
    }

    private static string? FormatMedicalNotes(string allergies, string medicalNotes)
    {
        var sections = new List<string>();

        if (!string.IsNullOrWhiteSpace(allergies))
        {
            sections.Add($"Alergias: {allergies.Trim()}");
        }

        if (!string.IsNullOrWhiteSpace(medicalNotes))
        {
            sections.Add($"Notas médicas: {medicalNotes.Trim()}");
        }

        return sections.Count == 0 ? null : string.Join(Environment.NewLine, sections);
    }
}
