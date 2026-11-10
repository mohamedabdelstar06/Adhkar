namespace Zad.Modules.Adhkar.Contracts.Adhkar;

public sealed record AdhkarDto(
    Guid Id,
    string Text,
    string Translation,
    string Reference,
    string Category,
    int DisplayOrder,
    bool IsActive);

