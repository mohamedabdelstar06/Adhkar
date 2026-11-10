using System.Text.Json;

namespace Zad.BuildingBlocks.Messaging.Registry;

public static class MessagingJson
{
    public static readonly JsonSerializerOptions Options =
        new(JsonSerializerDefaults.Web) { PropertyNameCaseInsensitive = true };
}
