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
    /// 接收 CloudEvent JSON
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Handle([FromBody] StorageEventData data,
                                            CancellationToken ct = default)
    {
        // log 下 header + raw body
        _logger.LogDebug("Headers: {Headers}", Request.Headers.ToDictionary(k => k.Key, v => v.Value.ToString()));
        _logger.LogDebug("Raw Body: {Body}", data.ToString());

        // 主程式
        var (thumbKey, imageId) = await _imageSvc.ProcessAsync(data, ct);
        await _sqlSvc.MarkDoneAsync(imageId, thumbKey, ct);

        return Ok();
    }
}
