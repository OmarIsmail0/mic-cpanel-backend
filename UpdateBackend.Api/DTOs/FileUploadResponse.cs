namespace UpdateBackend.Api.DTOs
{
    public class FileUploadResponse
    {
        public string OriginalName { get; set; } = string.Empty;
        public long Size { get; set; }
        public string MimeType { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string Filename { get; set; } = string.Empty;
        public string? Pdf { get; set; }
        public string? Video { get; set; }
    }
}
