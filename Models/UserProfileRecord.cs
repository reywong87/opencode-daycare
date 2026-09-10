using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace OpenDaycare.Models;

[Table("users")]
public sealed class UserProfileRecord : BaseModel
{
    [PrimaryKey("id", false)]
    public Guid Id { get; set; }

    [Column("daycare_id")]
    public Guid? DaycareId { get; set; }

    [Column("role")]
    public string Role { get; set; } = string.Empty;

    [Column("status")]
    public string Status { get; set; } = string.Empty;

    [Column("full_name")]
    public string FullName { get; set; } = string.Empty;
}
