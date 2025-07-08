using Google.Cloud.Storage.V1;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ResizeWorker.Services;

public sealed class FakeStorage : IStorage
{
    public Task DownloadAsync(string bucket, string objectName,
                              Stream destination, CancellationToken ct)
    {
        // 產一張 10×10 假圖
        using var img = new Image<Rgba32>(10, 10);
        img.SaveAsJpeg(destination);
        destination.Position = 0;
        return Task.CompletedTask;
    }

    public Task UploadAsync(string bucket, string objectName, string contentType,
                            Stream source, CancellationToken ct)
        => Task.CompletedTask; // no-op
}
