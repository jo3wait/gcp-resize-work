using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ResizeWork.Models;

public sealed record StorageEventData
{
    [JsonPropertyName("bucket")] public string Bucket { get; init; } = default!;
    [JsonPropertyName("name")] public string Name { get; init; } = default!;
    [JsonPropertyName("contentType")] public string ContentType { get; init; } = default!;
    [JsonPropertyName("metadata")] public Dictionary<string, string>? Metadata { get; init; }
}
