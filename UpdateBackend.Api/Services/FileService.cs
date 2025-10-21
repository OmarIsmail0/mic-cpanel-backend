using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System.IO;
using UpdateBackend.Api.Configurations;
using UpdateBackend.Api.DTOs;

namespace UpdateBackend.Api.Services
{
    public class FileService : IFileService
    {
        private readonly FileUploadSettings _settings;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<FileService> _logger;

        public FileService(IOptions<FileUploadSettings> settings, IWebHostEnvironment environment, ILogger<FileService> logger)
        {
            _settings = settings.Value;
            _environment = environment;
            _logger = logger;
        }

        public async Task<FileUploadResponse> UploadFileAsync(IFormFile file, string subfolder = "")
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File is empty or null");

            if (!IsValidFileType(file.FileName, file.ContentType))
                throw new ArgumentException($"Invalid file type. Allowed types: images, PDFs, videos");

            if (!IsValidFileSize(file.Length))
                throw new ArgumentException($"File too large. Maximum size is {_settings.MaxFileSize / 1024 / 1024}MB");

            var uploadPath = Path.Combine(_environment.WebRootPath, _settings.UploadPath, subfolder);
            Directory.CreateDirectory(uploadPath);

            var fileName = GenerateFileName(file.FileName, file.ContentType);
            var filePath = Path.Combine(uploadPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var fileUrl = GetFileUrl(fileName, subfolder);

            return new FileUploadResponse
            {
                OriginalName = file.FileName,
                Size = file.Length,
                MimeType = file.ContentType,
                Path = filePath,
                Url = fileUrl,
                Filename = fileName,
                Pdf = file.ContentType == "application/pdf" ? fileName : null,
                Video = file.ContentType.StartsWith("video/") ? fileName : null
            };
        }

        public async Task<List<FileUploadResponse>> UploadFilesAsync(IFormFileCollection files, string subfolder = "")
        {
            var results = new List<FileUploadResponse>();

            foreach (var file in files)
            {
                try
                {
                    var result = await UploadFileAsync(file, subfolder);
                    results.Add(result);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error uploading file {FileName}", file.FileName);
                    throw;
                }
            }

            return results;
        }

        public Task<bool> DeleteFileAsync(string fileName, string subfolder = "")
        {
            try
            {
                var filePath = Path.Combine(_environment.WebRootPath, _settings.UploadPath, subfolder, fileName);

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    return Task.FromResult(true);
                }
                return Task.FromResult(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting file {FileName}", fileName);
                return Task.FromResult(false);
            }
        }

        public Task<bool> FileExistsAsync(string fileName, string subfolder = "")
        {
            var filePath = Path.Combine(_environment.WebRootPath, _settings.UploadPath, subfolder, fileName);
            return Task.FromResult(File.Exists(filePath));
        }

        public string GetFileUrl(string fileName, string subfolder = "")
        {
            var url = $"uploads/{subfolder}/{fileName}".Replace("//", "/");
            return url;
        }

        public bool IsValidFileType(string fileName, string contentType)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();

            if (contentType.StartsWith("image/"))
                return _settings.AllowedImageExtensions.Contains(extension);

            if (contentType == "application/pdf")
                return _settings.AllowedDocumentExtensions.Contains(extension);

            if (contentType.StartsWith("video/"))
                return _settings.AllowedVideoExtensions.Contains(extension);

            return false;
        }

        public bool IsValidFileSize(long fileSize)
        {
            return fileSize <= _settings.MaxFileSize;
        }

        private string GenerateFileName(string originalFileName, string contentType)
        {
            var extension = Path.GetExtension(originalFileName);

            // For PDFs and videos, keep original filename
            if (contentType == "application/pdf" || contentType.StartsWith("video/"))
            {
                return originalFileName;
            }

            // For images, generate unique filename
            var uniqueSuffix = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + "-" + Guid.NewGuid().ToString("N")[..8];
            return $"image-{uniqueSuffix}{extension}";
        }
    }
}
