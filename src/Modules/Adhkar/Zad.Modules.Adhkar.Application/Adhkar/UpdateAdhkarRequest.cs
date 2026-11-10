namespace Zad.Modules.Adhkar.Application.Adhkar;

public sealed record UpdateAdhkarRequest(
    string Text,
    string Translation,
    string Reference,
    string Category,
    int DisplayOrder,
    bool IsActive);

