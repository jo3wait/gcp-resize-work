using CloudNative.CloudEvents.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using ResizeWork.Models;
using ResizeWork.Services;
using ResizeWorker.Services;
using System.Text;
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
    /// 接收 CloudEvent JSON
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Handle(/*[FromBody] JsonElement body,*/
                                            CancellationToken ct = default)
    {
        _logger.LogWarning("Content-Type: {ContentType}", Request.ContentType);
        _logger.LogWarning("Headers: {Headers}", Request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString()));

        // 1. Enable buffering 並讀取原始 body
        Request.EnableBuffering();
        string rawBody;
        using (var reader = new StreamReader(Request.Body, Encoding.UTF8, leaveOpen: true))
        {
            rawBody = await reader.ReadToEndAsync(ct);
            Request.Body.Position = 0;
        }
        _logger.LogDebug("Raw request body: {RawBody}", rawBody);

        // 2. 解析 JSON
        var data = JsonSerializer.Deserialize<StorageEventData>(rawBody, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        // 3. 主程式
        var (thumbKey, imageId) = await _imageSvc.ProcessAsync(data, ct);
        await _sqlSvc.MarkDoneAsync(imageId, thumbKey, ct);

        return Ok();
    }
}
