namespace UpdateBackend.Api.Configurations
{
    public class FileUploadSettings
    {
        public long MaxFileSize { get; set; } = 5242880; // 5MB
        public string[] AllowedImageExtensions { get; set; } = { ".jpg", ".jpeg", ".png", ".gif" };
        public string[] AllowedVideoExtensions { get; set; } = { ".mp4", ".avi", ".mov" };
        public string[] AllowedDocumentExtensions { get; set; } = { ".pdf" };
        public string UploadPath { get; set; } = "wwwroot/uploads";
    }
}
