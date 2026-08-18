namespace OpenDaycare.Components.Shared;

public sealed record Kid(
    string Slug,
    string FullName,
    string Initials,
    string AvatarBackgroundClass,
    string AvatarTextClass,
    int Age,
    string Room,
    DateOnly BirthDate,
    string EnrollmentLabel,
    string? Notes,
    IReadOnlyList<LinkedParent> Parents);

public sealed record LinkedParent(
    string FullName,
    string Initials,
    string Relationship,
    string InvitationStatus,
    string AvatarBackgroundClass,
    string StatusBackgroundClass,
    string StatusTextClass);

public static class KidCatalog
{
    public static IReadOnlyList<Kid> All { get; } =
    [
        new(
            "mateo-fernandez", "Mateo Fernández", "M", "bg-[#A9D9E8]", "text-[#1F7A93]", 3, "Soles",
            new DateOnly(2022, 3, 12), "feb 2025",
            "Alergia al maní. Evitar frutos secos. Lleva inhalador en la mochila.",
            [
                new("Lucía Fernández", "L", "Mamá", "Activa", "bg-[#C9B6E8]", "bg-[#CFEBD8]", "text-[#3E9B6C]"),
                new("Diego Fernández", "D", "Papá", "Pendiente", "bg-[#A9C7E8]", "bg-[#F7E7A6]", "text-[#9A7B1E]")
            ]),
        new(
            "sofia-mendez", "Sofía Méndez", "S", "bg-[#F4B8CC]", "text-[#C44A7A]", 2, "Soles",
            new DateOnly(2023, 7, 8), "mar 2025", null,
            [
                new("Marina Méndez", "M", "Mamá", "Activa", "bg-[#F4B8CC]", "bg-[#CFEBD8]", "text-[#3E9B6C]")
            ]),
        new(
            "benjamin-ruiz", "Benjamín Ruiz", "B", "bg-[#B9DEC4]", "text-[#3E8B62]", 3, "Soles",
            new DateOnly(2022, 9, 20), "ene 2025", null,
            [
                new("Clara Ruiz", "C", "Mamá", "Activa", "bg-[#F4DC8E]", "bg-[#CFEBD8]", "text-[#3E9B6C]"),
                new("Pablo Ruiz", "P", "Papá", "Activa", "bg-[#A9D9E8]", "bg-[#CFEBD8]", "text-[#3E9B6C]")
            ]),
        new(
            "valentina-soto", "Valentina Soto", "V", "bg-[#F4DC8E]", "text-[#9A7B1E]", 2, "Soles",
            new DateOnly(2023, 2, 17), "abr 2025", null,
            []),
        new(
            "tomas-diaz", "Tomás Díaz", "T", "bg-[#C9B6E8]", "text-[#7B5FC0]", 3, "Soles",
            new DateOnly(2022, 5, 30), "feb 2025", "Intolerancia a la lactosa. Enviar colaciones sin lácteos.",
            [
                new("Nicolás Díaz", "N", "Papá", "Activa", "bg-[#A9C7E8]", "bg-[#CFEBD8]", "text-[#3E9B6C]")
            ]),
        new(
            "emma-castro", "Emma Castro", "E", "bg-[#F4B8CC]", "text-[#C44A7A]", 2, "Soles",
            new DateOnly(2023, 10, 4), "may 2025", null,
            [
                new("Elena Castro", "E", "Mamá", "Pendiente", "bg-[#C9B6E8]", "bg-[#F7E7A6]", "text-[#9A7B1E]")
            ]),
        new(
            "lucas-romero", "Lucas Romero", "L", "bg-[#A9D9E8]", "text-[#1F7A93]", 3, "Soles",
            new DateOnly(2022, 1, 25), "mar 2025", null,
            [
                new("Silvia Romero", "S", "Mamá", "Activa", "bg-[#F4B8CC]", "bg-[#CFEBD8]", "text-[#3E9B6C]")
            ]),
        new(
            "olivia-vega", "Olivia Vega", "O", "bg-[#B9DEC4]", "text-[#3E8B62]", 2, "Soles",
            new DateOnly(2023, 4, 11), "jun 2025", null,
            [
                new("Daniel Vega", "D", "Papá", "Activa", "bg-[#A9C7E8]", "bg-[#CFEBD8]", "text-[#3E9B6C]")
            ])
    ];

    public static Kid? FindBySlug(string? slug) => All.FirstOrDefault(kid =>
        string.Equals(kid.Slug, slug, StringComparison.OrdinalIgnoreCase));
}
