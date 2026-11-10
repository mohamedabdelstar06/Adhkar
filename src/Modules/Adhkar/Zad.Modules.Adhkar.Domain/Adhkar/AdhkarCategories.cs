namespace Zad.Modules.Adhkar.Domain.Adhkar;

public static class AdhkarCategories
{
    public const string Tasbih = "tasbih";
    public const string Tahmid = "tahmid";
    public const string Takbir = "takbir";
    public const string Tahlil = "tahlil";
    public const string Hawqala = "hawqala";
    public const string Salawat = "salawat";

    public static readonly IReadOnlyList<string> All =
        [Tasbih, Tahmid, Takbir, Tahlil, Hawqala, Salawat];

    public static bool IsValid(string category) =>
        All.Contains(category.Trim().ToLowerInvariant(), StringComparer.Ordinal);
}

