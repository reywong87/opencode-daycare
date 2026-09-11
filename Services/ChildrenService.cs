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

        var child = new Child
        {
            RoomId = request.RoomId,
            FullName = request.FullName.Trim(),
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
