using Google.Cloud.Storage.V1;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ResizeWorker.Services;

public sealed class GcsStorage : IStorage
{
    private readonly StorageClient _client;

    public GcsStorage(StorageClient client) => _client = client;

    public Task DownloadAsync(string bucket, string objectName,
                              Stream destination, CancellationToken ct) =>
        _client.DownloadObjectAsync(bucket, objectName, destination,
                                    options: null, progress: null, cancellationToken: ct);

    public Task UploadAsync(string bucket, string objectName, string contentType,
                            Stream source, CancellationToken ct) =>
        _client.UploadObjectAsync(bucket, objectName, contentType, source,
                                  options: null, progress: null, cancellationToken: ct);
}
