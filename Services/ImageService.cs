using ResizeWork.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;

namespace ResizeWorker.Services;

public sealed class ImageService
{
    private readonly IStorage _storage;
    private readonly long? _targetBytes;
    private readonly int _minQuality;

    public ImageService(IStorage storage)
    {
        _storage = storage;
        if (long.TryParse(Environment.GetEnvironmentVariable("TARGET_SIZE_MB"), out var mb))
            _targetBytes = mb * 1_048_576L;            // MB → Bytes
        _minQuality = int.TryParse(Environment.GetEnvironmentVariable("MIN_JPEG_QUALITY"), out var q) ? q : 60;
    }

    public async Task<(string thumbPath, string imageId)> ProcessAsync(
        StorageEventData ev, CancellationToken ct)
    {
        var imageId = ev.Metadata?["imageid"]
                   ?? Path.GetFileNameWithoutExtension(ev.Name);

        // 1. 下載原圖
        await using var src = new MemoryStream();
        await _storage.DownloadAsync(ev.Bucket, ev.Name, src, ct);
        var origBytes = src.Length;
        src.Position = 0;

        // 若未設定 TARGET_SIZE_MB => fallback 固定解析度
        if (_targetBytes is null)
            return await FixedResizeAsync(src, ev, imageId, ct);

        // 2. 第一次按比例縮圖
        using var img = await Image.LoadAsync(src, ct);
        var scale = Math.Sqrt((double)_targetBytes.Value / origBytes);
        if (scale < 1.0)
        {
            var newW = (int)(img.Width * scale);
            var newH = (int)(img.Height * scale);
            img.Mutate(x => x.Resize(newW, newH));
        }

        // 3. 迭代調整品質 / 尺寸
        var quality = 90;
        MemoryStream thumbStream;
        while (true)
        {
            thumbStream = new MemoryStream();
            var enc = new JpegEncoder { Quality = quality };
            await img.SaveAsJpegAsync(thumbStream, enc, ct);

            if (thumbStream.Length <= _targetBytes || quality <= _minQuality)
            {
                if (thumbStream.Length <= _targetBytes) break;

                // 尺寸再降 0.9 倍
                img.Mutate(x => x.Resize((int)(img.Width * 0.9), (int)(img.Height * 0.9)));
                quality = 90;              // 重設品質，再嘗試
                continue;
            }
            quality -= 10;                // 品質再降
        }

        // 4. 上傳縮圖
        thumbStream.Position = 0;
        var thumbKey = $"thumbs/{imageId}_{_targetBytes / 1_048_576}MB.jpg";
        await _storage.UploadAsync(ev.Bucket, thumbKey, "image/jpeg", thumbStream, ct);

        return (thumbKey, imageId);
    }

    private async Task<(string, string)> FixedResizeAsync(
        Stream src, StorageEventData ev, string imageId, CancellationToken ct)
    {
        var maxW = int.Parse(Environment.GetEnvironmentVariable("TARGET_MAX_W") ?? "1024");
        var maxH = int.Parse(Environment.GetEnvironmentVariable("TARGET_MAX_H") ?? "768");

        using var img = await Image.LoadAsync(src, ct);
        img.Mutate(x => x.Resize(new ResizeOptions
        {
            Mode = ResizeMode.Max,
            Size = new Size(maxW, maxH)
        }));

        using var th = new MemoryStream();
        await img.SaveAsJpegAsync(th, ct);
        th.Position = 0;

        var key = $"thumbs/{imageId}_{maxW}.jpg";
        await _storage.UploadAsync(ev.Bucket, key, "image/jpeg", th, ct);
        return (key, imageId);
    }
}
