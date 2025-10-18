using Microsoft.AspNetCore.Http;
using UpdateBackend.Api.DTOs;

namespace UpdateBackend.Api.Services
{
    public interface IFileService
    {
        Task<FileUploadResponse> UploadFileAsync(IFormFile file, string subfolder = "");
        Task<List<FileUploadResponse>> UploadFilesAsync(IFormFileCollection files, string subfolder = "");
        Task<bool> DeleteFileAsync(string fileName, string subfolder = "");
        Task<bool> FileExistsAsync(string fileName, string subfolder = "");
        string GetFileUrl(string fileName, string subfolder = "");
        bool IsValidFileType(string fileName, string contentType);
        bool IsValidFileSize(long fileSize);
    }
}
