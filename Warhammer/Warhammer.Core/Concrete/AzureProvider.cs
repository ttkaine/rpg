using System;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Warhammer.Core.Abstract;
using Warhammer.Core.Entities;

namespace Warhammer.Core.Concrete
{
    public class AzureProvider : IAzureProvider
    {
        private const string ContainerName = "images";

        readonly IAdminSettingsProvider _settings;

        public AzureProvider(IAdminSettingsProvider settings)
        {
            _settings = settings;
        }


        public CloudStorageAccount GetStorageAccount()
        {
            CloudStorageAccount storageAccount;
            const string message = "Invalid storage account information provided. Please confirm the AccountName and AccountKey are valid in the DB.";
            string storageConnectionString = _settings.GetAdminSetting(AdminSettingName.AzureStorageConnectionString);
            try
            {
                storageAccount = CloudStorageAccount.Parse(storageConnectionString);
            }
            catch (FormatException)
            {
                Console.WriteLine(message);
                throw;
            }
            catch (ArgumentException)
            {
                Console.WriteLine(message);
                throw;
            }

            return storageAccount;
        }

        public CloudBlobClient GetBlobClient()
        {
            return GetStorageAccount().CreateCloudBlobClient();
        }

        public string SaveBlob(string blobName, byte[] data, string mimeType)
        {
            CloudBlobClient blobClient = GetBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference(ContainerName);
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(blobName);
            blockBlob.Properties.ContentType = mimeType;
            blockBlob.Properties.CacheControl = "public, max-age=31536000";
            blockBlob.UploadFromByteArray(data, 0, data.Length, null, new BlobRequestOptions { });
            return blockBlob.Uri.AbsoluteUri;
        }

        public ICloudBlob GetImageBlobReference(string fileIdentifier)
        {
            CloudBlobClient blobClient = GetBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference(ContainerName);
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(fileIdentifier);
            return blockBlob;
        }

        public string CreateImageBlob(byte[] bytes, string mimeType = "image/jpeg")
        {
            Guid guid = Guid.NewGuid();
            string key = guid.ToString();
            SaveBlob(key, bytes, mimeType);
            return key;
        }
    }
}