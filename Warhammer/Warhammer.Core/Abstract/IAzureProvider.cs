using Microsoft.WindowsAzure.Storage.Blob;

namespace Warhammer.Core.Abstract
{
    public interface IAzureProvider
    {
        string SaveBlob(string blobName, byte[] data, string mimeType);
        ICloudBlob GetImageBlobReference(string fileIdentifier);
        string CreateImageBlob(byte[] bytes, string mimeType = "image/jpeg");
    }
}