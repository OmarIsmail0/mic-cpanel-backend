using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using UpdateBackend.Api.DTOs;
using UpdateBackend.Api.Repositories;

namespace UpdateBackend.Api.Controllers
{
    [ApiController]
    [Route("api/admin")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminRepository _adminRepository;
        private readonly ILogger<AdminController> _logger;
        private readonly IConfiguration _configuration;

        public AdminController(IAdminRepository adminRepository, ILogger<AdminController> logger, IConfiguration configuration)
        {
            _adminRepository = adminRepository;
            _logger = logger;
            _configuration = configuration;
        }

        /// <summary>
        /// Get dashboard data
        /// </summary>
        [HttpGet("dashboard")]
        public ActionResult<ApiResponse<object>> GetDashboard()
        {
            try
            {
                // Get basic system information
                var systemInfo = new
                {
                    dotnetVersion = Environment.Version.ToString(),
                    platform = Environment.OSVersion.Platform.ToString(),
                    uptime = Environment.TickCount64 / 1000, // Convert to seconds
                    memory = GC.GetTotalMemory(false),
                    timestamp = DateTime.UtcNow.ToString("O")
                };

                // Check if MIC website path exists
                var micWebsitePath = _configuration["MIC_WEBSITE_PATH"];
                var micWebsiteStatus = "not_configured";
                var micWebsiteFiles = new List<string>();

                if (!string.IsNullOrWhiteSpace(micWebsitePath))
                {
                    try
                    {
                        if (Directory.Exists(micWebsitePath))
                        {
                            var files = Directory.GetFileSystemEntries(micWebsitePath).Take(10).Select(Path.GetFileName).Where(f => f != null).Cast<string>().ToList();
                            micWebsiteStatus = "accessible";
                            micWebsiteFiles = files;
                        }
                        else
                        {
                            micWebsiteStatus = "inaccessible";
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error accessing MIC website path");
                        micWebsiteStatus = "inaccessible";
                    }
                }

                var dashboardData = new
                {
                    systemInfo,
                    micWebsite = new
                    {
                        path = micWebsitePath,
                        status = micWebsiteStatus,
                        files = micWebsiteFiles
                    },
                    database = new
                    {
                        status = "connected", // Assuming MongoDB is connected if this route is reached
                        collections = new List<string>() // You can add collection info here
                    }
                };

                return Ok(ApiResponse<object>.SuccessResponse(dashboardData, "Dashboard data retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching dashboard data");
                return StatusCode(500, ApiResponse<object>.ErrorResponse(
                    "Server error while fetching dashboard data",
                    ex.Message));
            }
        }

        /// <summary>
        /// Get MIC website files
        /// </summary>
        [HttpGet("website/files")]
        public ActionResult<ApiResponse<object>> GetWebsiteFiles([FromQuery] string directory = "")
        {
            try
            {
                var micWebsitePath = _configuration["MIC_WEBSITE_PATH"];
                if (string.IsNullOrWhiteSpace(micWebsitePath))
                {
                    return BadRequest(ApiResponse<object>.ErrorResponse("MIC website path not configured"));
                }

                var fullPath = Path.Combine(micWebsitePath, directory);

                // Security check - ensure path is within MIC_WEBSITE_PATH
                if (!Path.GetFullPath(fullPath).StartsWith(Path.GetFullPath(micWebsitePath)))
                {
                    return StatusCode(403, ApiResponse<object>.ErrorResponse("Access denied"));
                }

                if (!Directory.Exists(fullPath))
                {
                    return NotFound(ApiResponse<object>.ErrorResponse("Directory not found"));
                }

                var entries = Directory.GetFileSystemEntries(fullPath);
                var fileList = entries.Select(entry =>
                {
                    var fileInfo = new FileInfo(entry);
                    return new
                    {
                        name = fileInfo.Name,
                        type = fileInfo.Attributes.HasFlag(FileAttributes.Directory) ? "directory" : "file",
                        path = Path.GetRelativePath(micWebsitePath, entry).Replace("\\", "/")
                    };
                }).ToList();

                var result = new
                {
                    files = fileList,
                    currentPath = directory,
                    parentPath = Path.GetDirectoryName(directory)?.Replace("\\", "/") ?? ""
                };

                return Ok(ApiResponse<object>.SuccessResponse(result, "Files retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading directory {Directory}", directory);
                return StatusCode(500, ApiResponse<object>.ErrorResponse("Error reading directory", ex.Message));
            }
        }

        /// <summary>
        /// Get file content
        /// </summary>
        [HttpGet("website/file")]
        public async Task<ActionResult<ApiResponse<object>>> GetFileContent([FromQuery] string filePath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(filePath))
                {
                    return BadRequest(ApiResponse<object>.ErrorResponse("File path is required"));
                }

                var micWebsitePath = _configuration["MIC_WEBSITE_PATH"];
                if (string.IsNullOrWhiteSpace(micWebsitePath))
                {
                    return BadRequest(ApiResponse<object>.ErrorResponse("MIC website path not configured"));
                }

                var fullPath = Path.Combine(micWebsitePath, filePath);

                // Security check
                if (!Path.GetFullPath(fullPath).StartsWith(Path.GetFullPath(micWebsitePath)))
                {
                    return StatusCode(403, ApiResponse<object>.ErrorResponse("Access denied"));
                }

                if (!System.IO.File.Exists(fullPath))
                {
                    return NotFound(ApiResponse<object>.ErrorResponse("File not found"));
                }

                var content = await System.IO.File.ReadAllTextAsync(fullPath);
                var fileInfo = new FileInfo(fullPath);

                var result = new
                {
                    content,
                    stats = new
                    {
                        size = fileInfo.Length,
                        modified = fileInfo.LastWriteTime,
                        isFile = !fileInfo.Attributes.HasFlag(FileAttributes.Directory),
                        isDirectory = fileInfo.Attributes.HasFlag(FileAttributes.Directory)
                    }
                };

                return Ok(ApiResponse<object>.SuccessResponse(result, "File content retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading file {FilePath}", filePath);
                return StatusCode(500, ApiResponse<object>.ErrorResponse("Error reading file", ex.Message));
            }
        }

        /// <summary>
        /// Update file content
        /// </summary>
        [HttpPut("website/file")]
        public async Task<ActionResult<ApiResponse<object>>> UpdateFileContent([FromBody] UpdateFileRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.FilePath) || request.Content == null)
                {
                    return BadRequest(ApiResponse<object>.ErrorResponse("File path and content are required"));
                }

                var micWebsitePath = _configuration["MIC_WEBSITE_PATH"];
                if (string.IsNullOrWhiteSpace(micWebsitePath))
                {
                    return BadRequest(ApiResponse<object>.ErrorResponse("MIC website path not configured"));
                }

                var fullPath = Path.Combine(micWebsitePath, request.FilePath);

                // Security check
                if (!Path.GetFullPath(fullPath).StartsWith(Path.GetFullPath(micWebsitePath)))
                {
                    return StatusCode(403, ApiResponse<object>.ErrorResponse("Access denied"));
                }

                await System.IO.File.WriteAllTextAsync(fullPath, request.Content);

                return Ok(ApiResponse<object>.SuccessResponse(new { message = "File updated successfully" }, "File updated successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating file {FilePath}", request.FilePath);
                return StatusCode(500, ApiResponse<object>.ErrorResponse("Error updating file", ex.Message));
            }
        }
    }

    public class UpdateFileRequest
    {
        public string FilePath { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
    }
}
