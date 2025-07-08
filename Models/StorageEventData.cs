using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ResizeWork.Models;

public sealed record StorageEventData
{
    /// <summary>
    /// 被觸發事件的 Bucket 名稱
    /// </summary>
    [JsonPropertyName("bucket")]
    public string Bucket { get; init; } = default!;

    /// <summary>
    /// 檔案在 Bucket 中的完整路徑（含資料夾）
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; init; } = default!;

    /// <summary>
    /// 檔案的 MIME Type，例如 image/jpeg
    /// </summary>
    [JsonPropertyName("contentType")]
    public string ContentType { get; init; } = default!;

    /// <summary>
    /// 檔案的大小（Bytes），GCS 回傳的是字串，所以用 long? 來接
    /// </summary>
    [JsonPropertyName("size")]
    public long? Size { get; init; }

    /// <summary>
    /// 檔案第一次建立的 UTC 時間
    /// </summary>
    [JsonPropertyName("timeCreated")]
    public DateTimeOffset? TimeCreated { get; init; }

    /// <summary>
    /// 最後更新的 UTC 時間
    /// </summary>
    [JsonPropertyName("updated")]
    public DateTimeOffset? Updated { get; init; }

    /// <summary>
    /// 內建的 generation 編號（可用來做版本比對）
    /// </summary>
    [JsonPropertyName("generation")]
    public string? Generation { get; init; }

    /// <summary>
    /// metadata version
    /// </summary>
    [JsonPropertyName("metageneration")]
    public string? Metageneration { get; init; }
}
