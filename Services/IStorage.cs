using Google.Cloud.Storage.V1;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ResizeWorker.Services;

public interface IStorage
{
    Task DownloadAsync(string bucket, string objectName,
                       Stream destination, CancellationToken ct);

    Task UploadAsync(string bucket, string objectName, string contentType,
                     Stream source, UploadObjectOptions options,CancellationToken ct);
}
