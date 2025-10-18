using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using System.Text.Json;
using UpdateBackend.Api.DTOs;
using UpdateBackend.Api.Models;
using UpdateBackend.Api.Repositories;

namespace UpdateBackend.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FormsController : ControllerBase
    {
        private readonly IFormRepository _formRepository;
        private readonly ILogger<FormsController> _logger;

        public FormsController(IFormRepository formRepository, ILogger<FormsController> logger)
        {
            _formRepository = formRepository;
            _logger = logger;
        }

        /// <summary>
        /// Get all forms
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<Form>>>> GetForms()
        {
            try
            {
                var forms = await _formRepository.GetAllAsync();
                var formsList = forms.OrderByDescending(f => f.CreatedAt).ToList();

                return Ok(ApiResponse<IEnumerable<Form>>.SuccessResponse(
                    formsList,
                    "Forms retrieved successfully",
                    formsList.Count));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching forms");
                return StatusCode(500, ApiResponse<IEnumerable<Form>>.ErrorResponse(
                    "Error fetching forms",
                    ex.Message));
            }
        }

        /// <summary>
        /// Create a new form
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ApiResponse<Form>>> CreateForm(CreateFormRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.FormName))
                {
                    return BadRequest(ApiResponse<Form>.ErrorResponse("Form name is required"));
                }

                if (request.FormData == null)
                {
                    return BadRequest(ApiResponse<Form>.ErrorResponse("Form data is required"));
                }

                // Convert FormData to BsonDocument
                var jsonString = JsonSerializer.Serialize(request.FormData);
                var formDataBson = BsonDocument.Parse(jsonString);

                var form = new Form
                {
                    FormName = request.FormName,
                    FormData = formDataBson,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var createdForm = await _formRepository.CreateAsync(form);

                return StatusCode(201, ApiResponse<Form>.SuccessResponse(
                    createdForm,
                    "Form created successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating form");
                return StatusCode(500, ApiResponse<Form>.ErrorResponse(
                    "Error creating form",
                    ex.Message));
            }
        }

        /// <summary>
        /// Delete a form by ID
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<Form>>> DeleteForm(string id)
        {
            try
            {
                var existingForm = await _formRepository.GetByIdAsync(id);
                if (existingForm == null)
                {
                    return NotFound(ApiResponse<Form>.ErrorResponse("Form not found"));
                }

                var deleted = await _formRepository.DeleteAsync(id);
                if (!deleted)
                {
                    return StatusCode(500, ApiResponse<Form>.ErrorResponse("Error deleting form"));
                }

                return Ok(ApiResponse<Form>.SuccessResponse(
                    existingForm,
                    "Form deleted successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting form");
                return StatusCode(500, ApiResponse<Form>.ErrorResponse(
                    "Error deleting form",
                    ex.Message));
            }
        }
    }

    public class CreateFormRequest
    {
        public string FormName { get; set; } = string.Empty;
        public object FormData { get; set; } = new object();
    }
}
