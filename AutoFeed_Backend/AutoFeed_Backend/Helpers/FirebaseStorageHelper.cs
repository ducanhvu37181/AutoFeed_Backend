using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Threading.Tasks;

namespace AutoFeed_Backend.Helpers
{
    public static class FirebaseStorageHelper
    {
        // Uploads file to Firebase Storage (GCS) and returns a signed URL valid for the given ttl.
        public static async Task<string> UploadFileAndGetSignedUrlAsync(IFormFile file, string bucketName, string serviceAccountPath, TimeSpan urlTtl)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("file is empty", nameof(file));

            if (string.IsNullOrWhiteSpace(bucketName))
                throw new ArgumentException("bucketName is required", nameof(bucketName));

            if (string.IsNullOrWhiteSpace(serviceAccountPath))
                throw new ArgumentException("serviceAccountPath is required", nameof(serviceAccountPath));

            var credential = GoogleCredential.FromFile(serviceAccountPath);
            var storageClient = StorageClient.Create(credential);

            var ext = Path.GetExtension(file.FileName) ?? string.Empty;
            var objectName = $"avatars/{Guid.NewGuid()}{ext}";

            using var ms = new MemoryStream();
            await file.CopyToAsync(ms);
            ms.Position = 0;

            // generate a token so the object can be accessed via Firebase download URL
            var token = Guid.NewGuid().ToString();

            var storageObject = new Google.Apis.Storage.v1.Data.Object
            {
                Bucket = bucketName,
                Name = objectName,
                ContentType = file.ContentType,
                Metadata = new System.Collections.Generic.Dictionary<string, string>
                {
                    { "firebaseStorageDownloadTokens", token }
                }
            };

            // upload with metadata
            await storageClient.UploadObjectAsync(storageObject, ms);

            // build Firebase download URL format
            var encodedName = Uri.EscapeDataString(objectName);
            return $"https://firebasestorage.googleapis.com/v0/b/{bucketName}/o/{encodedName}?alt=media&token={token}";
        }
    }
}
