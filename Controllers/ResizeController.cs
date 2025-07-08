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

        Request.EnableBuffering();

        // 2) 讀原始 Body（不指定 model binding）
        string rawBody;
        using (var reader = new StreamReader(Request.Body, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, leaveOpen: true))
        {
            rawBody = await reader.ReadToEndAsync();
            // 如果後面還要重用 Request.Body，可以先重設 Position：
            Request.Body.Position = 0;
        }

        _logger.LogWarning("Raw request body: {RawBody}", rawBody);

        // 3) 接下來就可以根據 rawBody 內容自行決定要不要 JsonDocument.Parse(rawBody) 
        //    或是直接把字串貼到 log 裡面，確認到底傳進來的格式長什麼樣子。
        _logger.LogWarning("Headers: {Headers}", JsonDocument.Parse(rawBody));

        //// log 下 header + raw body
        //_logger.LogWarning("Headers: {Headers}", Request.Headers.ToDictionary(k => k.Key, v => v.Value.ToString()));
        //_logger.LogWarning("Raw Body: {Body}", body.GetRawText());

        //StorageEventData? data = null;

        //// 解析 CloudEvent JSON

        //data = JsonSerializer.Deserialize<StorageEventData>(body.GetRawText());

        //// 主程式
        //var (thumbKey, imageId) = await _imageSvc.ProcessAsync(data, ct);
        //await _sqlSvc.MarkDoneAsync(imageId, thumbKey, ct);

        return Ok();
    }
}
