using Microsoft.AspNetCore.Http;

namespace UpdateBackend.Api.DTOs
{
    public class FileUploadRequest
    {
        /// <summary>
        /// The file to upload
        /// </summary>
        public IFormFile? File { get; set; }

        /// <summary>
        /// The ID of the page to update
        /// </summary>
        public string? PageId { get; set; }

        /// <summary>
        /// The specific field path to update (e.g., "sections.mediaImages.0").
        /// This field will be updated with the file URL.
        /// </summary>
        public string? FieldPath { get; set; }
    }
}
