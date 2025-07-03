using Microsoft.AspNetCore.Mvc;
using ResizeWork.Models;
using ResizeWork.Services;
using ResizeWorker.Services;
using System.Text.Json;

namespace ResizeWork.Controllers;

[ApiController]
[Route("/")]
public sealed class ResizeController : ControllerBase
{
    private const string GcsFinalizeEventType = "google.cloud.storage.object.v1.finalized";

    private readonly ImageService _imageSvc;
    private readonly SqlService _sqlSvc;
    private readonly ILogger<ResizeController> _logger;

    public ResizeController(ImageService img, SqlService db, ILogger<ResizeController> logger)
        => (_imageSvc, _sqlSvc, _logger) = (img, db, logger);

    /// <summary>
    /// 接收 CloudEvent（整包 JSON），只取 data 區段
    /// </summary>
    [HttpPost]
    [Consumes("application/cloudevents+json")]
    public async Task<IActionResult> Handle([FromBody] JsonElement body,
                                            CancellationToken ct = default)
    {
        // 1) log 下 header + raw body
        _logger.LogDebug("Headers: {Headers}", Request.Headers.ToDictionary(k => k.Key, v => v.Value.ToString()));
        _logger.LogDebug("Raw Body: {Body}", body.GetRawText());

        // 2) CloudEvent 'type' 檢查
        if (!body.TryGetProperty("type", out var typeProp) ||
            !string.Equals(typeProp.GetString(), GcsFinalizeEventType, StringComparison.Ordinal))
        {
            _logger.LogWarning("Received non-finalize event: {Type}", typeProp.GetString());
            return BadRequest("Not a GCS finalize event");
        }

        // 3) 取 data 區段，Deserialize 時開 Case‑Insensitive
        StorageEventData payload;
        try
        {
            payload = body
                .GetProperty("data")
                .Deserialize<StorageEventData>(new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                })!;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to deserialize StorageEventData");
            return BadRequest("Invalid CloudEvent data payload");
        }

        // 4) 主程式
        var (thumbKey, imageId) = await _imageSvc.ProcessAsync(payload, ct);
        await _sqlSvc.MarkDoneAsync(imageId, thumbKey, ct);

        return Ok();
    }
}
