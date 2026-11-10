using Zad.Modules.Adhkar.Contracts.Adhkar;

namespace Zad.Modules.Adhkar.Application.Adhkar;

internal static class AdhkarMappingExtensions
{
    internal static AdhkarDto ToDto(this Domain.Adhkar.Adhkar adhkar) => new(
        adhkar.Id.Value,
        adhkar.Text,
        adhkar.Translation,
        adhkar.Reference,
        adhkar.Category,
        adhkar.DisplayOrder,
        adhkar.IsActive);
}

