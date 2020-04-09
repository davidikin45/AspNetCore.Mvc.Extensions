using Microsoft.WindowsAzure.Storage.Blob;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions.AzureStorage
{
    public interface IAzureVideoStorage
    {
        Task<CloudBlockBlob> UploadVideoAsync(byte[] videoByteArray, string blobname,
          string title, string description);
        Task<bool> CheckIfBlobExistsAsync(string blobName);

        Task<IEnumerable<CloudBlockBlob>> ListVideoBlobsAsync(string prefix = null, bool includeSnapshots = false);

        Task DownloadVideoAsync(CloudBlockBlob cloudBlockBlob, Stream targetStream);
        Task OverwriteVideoAsync(CloudBlockBlob cloudBlockBlob, byte[] videoByteArray, string leaseId);
        Task DeleteVideoAsync(CloudBlockBlob cloudBlockBlob, string leaseId);

        Task UpdateMetadataAsync(CloudBlockBlob cloudBlockBlob, string title, string description, string leaseId);
        Task ReloadMetadataAsync(CloudBlockBlob cloudBlockBlob);
        (string title, string description) GetBlobMetadata(CloudBlockBlob cloudBlockBlob);

        string GetBlobUriWithSasToken(CloudBlockBlob cloudBlockBlob);

        Task<string> AcquireOneMinuteLeaseAsync(CloudBlockBlob cloudBlockBlob);
        Task RenewLeaseAsync(CloudBlockBlob cloudBlockBlob, string leaseId);
        Task ReleaseLeaseAsync(CloudBlockBlob cloudBlockBlob, string leaseId);

        Task<string> LoadLeaseInfoAsync(CloudBlockBlob cloudBlockBlob);

        Task CreateSnapshotAsync(CloudBlockBlob cloudBlockBlob);
        Task PromoteSnapshotAsync(CloudBlockBlob cloudBlockBlob);

        Task ArchiveVideoAsync(CloudBlockBlob cloudBlockBlob);
    }
}
 