using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ResizeWork.Models;

public sealed record StorageEventData
{
    [JsonPropertyName("bucket")] public string Bucket { get; init; } = default!;
    [JsonPropertyName("name")] public string Name { get; init; } = default!;

    // 新增：只為完整對映，不影響舊邏輯
    [JsonPropertyName("contentType")] public string? ContentType { get; init; }
    [JsonPropertyName("size")] public string? Size { get; init; }
    [JsonPropertyName("timeCreated")] public string? TimeCreated { get; init; }
    [JsonPropertyName("updated")] public string? Updated { get; init; }
}
