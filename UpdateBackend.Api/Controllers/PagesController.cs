using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using System.Text.Json;
using UpdateBackend.Api.DTOs;
using UpdateBackend.Api.Models;
using UpdateBackend.Api.Repositories;
using UpdateBackend.Api.Services;

namespace UpdateBackend.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PagesController : ControllerBase
    {
        private readonly IPageRepository _pageRepository;
        private readonly IFileService _fileService;
        private readonly ILogger<PagesController> _logger;

        public PagesController(IPageRepository pageRepository, IFileService fileService, ILogger<PagesController> logger)
        {
            _pageRepository = pageRepository;
            _fileService = fileService;
            _logger = logger;
        }

        /// <summary>
        /// Get all pages (limited for Swagger)
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<object>>>> GetPages()
        {
            try
            {
                var pages = await _pageRepository.GetAllAsync();
                var pagesList = pages.OrderByDescending(p => p.CreatedAt).Take(10).Select(p => new
                {
                    p.Id,
                    p.PageName,
                    p.CreatedAt,
                    p.UpdatedAt,
                    ImagesCount = p.Images?.Count ?? 0,
                    VideosCount = p.Videos?.Count ?? 0,
                    PdfsCount = p.Pdfs?.Count ?? 0,
                    HasSections = p.Sections != null
                }).ToList();

                return Ok(ApiResponse<IEnumerable<object>>.SuccessResponse(
                    pagesList,
                    "Pages retrieved successfully",
                    pagesList.Count));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching pages");
                return StatusCode(500, ApiResponse<IEnumerable<object>>.ErrorResponse(
                    "Error fetching pages",
                    ex.Message));
            }
        }

        /// <summary>
        /// Get a specific page by page name
        /// </summary>
        [HttpGet("{pageName}")]
        public async Task<ActionResult<ApiResponse<Page>>> GetPage(string pageName)
        {
            try
            {
                var page = await _pageRepository.GetByPageNameAsync(pageName);
                if (page == null)
                {
                    return NotFound(ApiResponse<Page>.ErrorResponse("Page not found"));
                }

                return Ok(ApiResponse<Page>.SuccessResponse(page, "Page retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching page {PageName}", pageName);
                return StatusCode(500, ApiResponse<Page>.ErrorResponse(
                    "Error fetching page",
                    ex.Message));
            }
        }

        /// <summary>
        /// Get a specific page by ID with full details
        /// </summary>
        [HttpGet("details/{id}")]
        public async Task<ActionResult<ApiResponse<Page>>> GetPageById(string id)
        {
            try
            {
                var page = await _pageRepository.GetByIdAsync(id);
                if (page == null)
                {
                    return NotFound(ApiResponse<Page>.ErrorResponse("Page not found"));
                }

                return Ok(ApiResponse<Page>.SuccessResponse(page, "Page retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching page {PageId}", id);
                return StatusCode(500, ApiResponse<Page>.ErrorResponse(
                    "Error fetching page",
                    ex.Message));
            }
        }

        /// <summary>
        /// Create a new page with file uploads
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ApiResponse<Page>>> CreatePage([FromForm] CreatePageRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.PageName))
                {
                    return BadRequest(ApiResponse<Page>.ErrorResponse("Page name is required"));
                }

                // Process uploaded images
                var imageUrls = new List<string>();
                if (request.Images != null && request.Images.Count > 0)
                {
                    var uploadResults = await _fileService.UploadFilesAsync(request.Images, "images");
                    imageUrls = uploadResults.Select(r => r.Url).ToList();
                }

                // Parse sections data
                BsonDocument sectionsBson = new BsonDocument();
                if (!string.IsNullOrWhiteSpace(request.Sections))
                {
                    try
                    {
                        sectionsBson = BsonDocument.Parse(request.Sections);
                    }
                    catch (JsonException ex)
                    {
                        return BadRequest(ApiResponse<Page>.ErrorResponse("Invalid sections JSON format", ex.Message));
                    }
                }

                // Process images with sections data
                if (imageUrls.Count > 0)
                {
                    sectionsBson = ProcessImagesWithSections(sectionsBson, imageUrls);
                }

                // Store as JSON string to avoid serialization wrappers
                var jsonString = sectionsBson.ToJson();

                var page = new Page
                {
                    PageName = request.PageName,
                    Sections = jsonString,
                    Images = imageUrls,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var createdPage = await _pageRepository.CreateAsync(page);

                return StatusCode(201, ApiResponse<Page>.SuccessResponse(
                    createdPage,
                    "Page created successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating page");
                return StatusCode(500, ApiResponse<Page>.ErrorResponse(
                    "Error creating page",
                    ex.Message));
            }
        }

        /// <summary>
        /// Update a page by ID with file uploads
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<Page>>> UpdatePage(string id, [FromForm] UpdatePageRequest request)
        {
            try
            {
                var existingPage = await _pageRepository.GetByIdAsync(id);
                if (existingPage == null)
                {
                    return NotFound(ApiResponse<Page>.ErrorResponse("Page not found"));
                }

                // Process uploaded images
                var imageUrls = new List<string>();
                if (request.Images != null && request.Images.Count > 0)
                {
                    var uploadResults = await _fileService.UploadFilesAsync(request.Images, "images");
                    imageUrls = uploadResults.Select(r => r.Url).ToList();
                }

                // Handle sections update - convert to BsonDocument for manipulation
                BsonDocument sectionsToUpdate;
                if (existingPage.Sections is string sectionsJson)
                {
                    sectionsToUpdate = BsonDocument.Parse(sectionsJson);
                }
                else if (existingPage.Sections is BsonDocument existingBson)
                {
                    sectionsToUpdate = existingBson;
                }
                else
                {
                    sectionsToUpdate = BsonDocument.Parse(System.Text.Json.JsonSerializer.Serialize(existingPage.Sections));
                }

                if (!string.IsNullOrWhiteSpace(request.Sections))
                {
                    try
                    {
                        var newSections = BsonDocument.Parse(request.Sections);
                        // Merge new sections with existing ones
                        foreach (var element in newSections)
                        {
                            sectionsToUpdate[element.Name] = element.Value;
                        }
                    }
                    catch (JsonException ex)
                    {
                        return BadRequest(ApiResponse<Page>.ErrorResponse("Invalid sections JSON format", ex.Message));
                    }
                }

                // Process images with sections data
                if (imageUrls.Count > 0)
                {
                    sectionsToUpdate = ProcessImagesWithSections(sectionsToUpdate, imageUrls);
                }

                // Update page fields
                var updateData = new Dictionary<string, object>
                {
                    ["updatedAt"] = DateTime.UtcNow
                };

                if (!string.IsNullOrWhiteSpace(request.PageName))
                    updateData["page"] = request.PageName;

                // Store as JSON string to avoid serialization wrappers
                var jsonString = sectionsToUpdate.ToJson();

                // Always update sections (merged with existing)
                updateData["sections"] = jsonString;

                if (imageUrls.Count > 0)
                    updateData["images"] = imageUrls;

                // Apply updates
                foreach (var kvp in updateData)
                {
                    await _pageRepository.UpdateFieldAsync(id, kvp.Key, kvp.Value);
                }

                var updatedPage = await _pageRepository.GetByIdAsync(id);

                return Ok(ApiResponse<Page>.SuccessResponse(
                    updatedPage ?? existingPage,
                    "Page updated successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating page {PageId}", id);
                return StatusCode(500, ApiResponse<Page>.ErrorResponse(
                    "Error updating page",
                    ex.Message));
            }
        }

        /// <summary>
        /// Delete a page by page name
        /// </summary>
        [HttpDelete("{pageName}")]
        public async Task<ActionResult<ApiResponse<Page>>> DeletePage(string pageName)
        {
            try
            {
                var existingPage = await _pageRepository.GetByPageNameAsync(pageName);
                if (existingPage == null)
                {
                    return NotFound(ApiResponse<Page>.ErrorResponse("Page not found"));
                }

                var deleted = await _pageRepository.DeleteAsync(existingPage.Id);
                if (!deleted)
                {
                    return StatusCode(500, ApiResponse<Page>.ErrorResponse("Error deleting page"));
                }

                return Ok(ApiResponse<Page>.SuccessResponse(
                    existingPage,
                    "Page deleted successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting page {PageName}", pageName);
                return StatusCode(500, ApiResponse<Page>.ErrorResponse(
                    "Error deleting page",
                    ex.Message));
            }
        }

        /// <summary>
        /// Upload a single file and optionally update a page
        /// </summary>
        [HttpPost("upload")]
        public async Task<ActionResult<ApiResponse<object>>> UploadFile([FromForm] FileUploadRequest request)
        {
            try
            {
                if (request.File == null || request.File.Length == 0)
                {
                    return BadRequest(ApiResponse<object>.ErrorResponse("No file provided"));
                }

                var uploadResult = await _fileService.UploadFileAsync(request.File, "images");
                var fileUrl = uploadResult.Url;

                // If pageId is provided, update the page
                if (!string.IsNullOrWhiteSpace(request.PageId))
                {
                    var existingPage = await _pageRepository.GetByIdAsync(request.PageId);
                    if (existingPage == null)
                    {
                        return NotFound(ApiResponse<object>.ErrorResponse("Page not found"));
                    }

                    var updateData = new Dictionary<string, object>
                    {
                        ["updatedAt"] = DateTime.UtcNow
                    };

                    // Update the specific field path with the file URL
                    if (!string.IsNullOrWhiteSpace(request.FieldPath))
                    {
                        // Use MongoDB's $set operator to update the specific nested field
                        await _pageRepository.UpdateNestedFieldAsync(request.PageId, request.FieldPath, fileUrl);
                    }
                    else
                    {
                        return BadRequest(ApiResponse<object>.ErrorResponse("FieldPath is required for file upload"));
                    }

                    // Apply updates
                    foreach (var kvp in updateData)
                    {
                        await _pageRepository.UpdateFieldAsync(request.PageId, kvp.Key, kvp.Value);
                    }

                    var updatedPage = await _pageRepository.GetByIdAsync(request.PageId);

                    return Ok(ApiResponse<object>.SuccessResponse(new
                    {
                        file = uploadResult,
                        updatedPage = updatedPage
                    }, "File uploaded and page updated successfully"));
                }

                return Ok(ApiResponse<object>.SuccessResponse(new { file = uploadResult }, "File uploaded successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file");
                return StatusCode(500, ApiResponse<object>.ErrorResponse(
                    "Error uploading file",
                    ex.Message));
            }
        }

        private BsonDocument ProcessImagesWithSections(BsonDocument sections, IEnumerable<string> uploadedImages)
        {
            if (sections == null || !uploadedImages.Any())
                return sections;

            var sectionsJson = sections.ToJson();
            var imageList = uploadedImages.ToList();
            var imageIndex = 0;

            // Replace placeholder images with uploaded ones
            foreach (var imageUrl in imageList)
            {
                if (imageIndex < imageList.Count)
                {
                    sectionsJson = sectionsJson.Replace("placeholder", imageUrl, StringComparison.OrdinalIgnoreCase);
                    imageIndex++;
                }
            }

            try
            {
                return BsonDocument.Parse(sectionsJson);
            }
            catch
            {
                return sections;
            }
        }

        /// <summary>
        /// Helper method to update nested fields in a BsonDocument
        /// </summary>
        private void UpdateNestedFieldInBsonDocument(BsonDocument document, string fieldPath, object value)
        {
            var pathParts = fieldPath.Split('.');
            var current = document;

            // Navigate to the parent of the target field
            for (int i = 0; i < pathParts.Length - 1; i++)
            {
                var part = pathParts[i];

                // Check if this part is an array index
                if (int.TryParse(part, out int arrayIndex))
                {
                    // Ensure we have an array at this level
                    if (!current.Contains("array") || !current["array"].IsBsonArray)
                    {
                        current["array"] = new BsonArray();
                    }

                    var array = current["array"].AsBsonArray;

                    // Ensure the array has enough elements
                    while (array.Count <= arrayIndex)
                    {
                        array.Add(new BsonDocument());
                    }

                    // Ensure the element at this index is a document
                    if (!array[arrayIndex].IsBsonDocument)
                    {
                        array[arrayIndex] = new BsonDocument();
                    }

                    current = array[arrayIndex].AsBsonDocument;
                }
                else
                {
                    // Regular field
                    if (!current.Contains(part) || !current[part].IsBsonDocument)
                    {
                        current[part] = new BsonDocument();
                    }
                    current = current[part].AsBsonDocument;
                }
            }

            // Set the final field
            var finalField = pathParts[pathParts.Length - 1];
            current[finalField] = BsonValue.Create(value);
        }
    }

    public class CreatePageRequest
    {
        public string PageName { get; set; } = string.Empty;
        public string? Sections { get; set; }
        public IFormFileCollection? Images { get; set; }
    }

    public class UpdatePageRequest
    {
        public string? PageName { get; set; }
        public string? Sections { get; set; }
        public IFormFileCollection? Images { get; set; }
    }

}
