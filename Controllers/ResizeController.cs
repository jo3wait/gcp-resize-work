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
    private readonly ImageService _imageSvc;
    private readonly SqlService _sqlSvc;

    public ResizeController(ImageService img, SqlService db)
        => (_imageSvc, _sqlSvc) = (img, db);

    /// <summary>
    /// 接收 CloudEvent（整包 JSON），只取裡頭 data 區段
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Handle([FromBody] JsonElement body,
                                            CancellationToken ct = default)
    {
        // CloudEvent 'type' 檢查
        if (!body.TryGetProperty("type", out var typeProp) ||
            typeProp.GetString() != "google.cloud.storage.object.v1.finalized")
            return BadRequest("Not a GCS finalize event");

        // 取 data 區段 反序列化成 StorageEventData
        var payload = body.GetProperty("data")
                          .Deserialize<StorageEventData>(new JsonSerializerOptions
                          {
                              PropertyNameCaseInsensitive = true
                          })!;

        var (thumbKey, imageId) = await _imageSvc.ProcessAsync(payload, ct);
        await _sqlSvc.MarkDoneAsync(imageId, thumbKey, ct);

        return Ok();
    }
}
