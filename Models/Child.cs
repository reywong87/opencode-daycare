using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace OpenDaycare.Models;

[Table("children")]
public sealed class Child : BaseModel
{
    [PrimaryKey("id", false)]
    public Guid Id { get; set; }

    [Column("room_id")]
    public Guid RoomId { get; set; }

    [Column("full_name")]
    public string FullName { get; set; } = string.Empty;

    [Column("birth_date")]
    public DateOnly BirthDate { get; set; }

    [Column("enrolled_at")]
    public DateOnly EnrolledAt { get; set; }

    [Column("medical_notes")]
    public string? MedicalNotes { get; set; }

    [Column("photo_consent")]
    public bool PhotoConsent { get; set; }

    [Column("status")]
    public string Status { get; set; } = string.Empty;
}
