using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CompanyExpenses.Database.Data;
using CompanyExpenses.Models.Entities;

namespace CompanyExpenses.Api.Controllers;

/// <summary>
/// Controller for expense attachment management including upload, download, and deletion.
/// </summary>
[ApiController]
[Route("api/expenses/{expenseId}/[controller]")]
public class ExpenseAttachmentsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<ExpenseAttachmentsController> _logger;
    private readonly IConfiguration _configuration;
    private readonly string _uploadPath;
    private readonly long _maxFileSizeBytes;
    private readonly string[] _allowedFileTypes;

    public ExpenseAttachmentsController(
        AppDbContext context,
        ILogger<ExpenseAttachmentsController> logger,
        IConfiguration configuration)
    {
        _context = context;
        _logger = logger;
        _configuration = configuration;

        _uploadPath = _configuration["FileStorage:UploadPath"]
            ?? Path.Combine(Directory.GetCurrentDirectory(), "uploads", "receipts");

        _maxFileSizeBytes = _configuration.GetValue<long>("FileStorage:MaxFileSizeBytes", 10_485_760);

        var allowedTypesConfig = _configuration["FileStorage:AllowedFileTypes"]
            ?? "image/jpeg,image/jpg,image/png,image/gif";
        _allowedFileTypes = allowedTypesConfig.Split(',', StringSplitOptions.RemoveEmptyEntries);

        Directory.CreateDirectory(_uploadPath);
    }

    /// <summary>
    /// Retrieves all attachments for a specific expense.
    /// </summary>
    /// <param name="expenseId">The unique identifier of the expense.</param>
    /// <returns>A list of attachments metadata.</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ExpenseAttachmentDto>>> GetAttachments(Guid expenseId)
    {
        try
        {
            _logger.LogInformation("Fetching attachments for expense {ExpenseId}", expenseId);

            var expense = await _context.Expenses
                .Include(e => e.Attachments)
                .FirstOrDefaultAsync(e => e.Id == expenseId);

            if (expense == null)
            {
                _logger.LogWarning("Expense not found: {ExpenseId}", expenseId);
                return NotFound(new { message = "Expense not found" });
            }

            var attachments = expense.Attachments.Select(a => new ExpenseAttachmentDto
            {
                Id = a.Id,
                ExpenseId = a.ExpenseId,
                OriginalFileName = a.OriginalFileName,
                DataType = a.DataType,
                FileSize = a.FileSize,
                UploadedByUserId = a.UploadedByUserId,
                UploadedAt = a.UploadedAt
            });

            return Ok(attachments);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get attachments for expense {ExpenseId}", expenseId);
            return StatusCode(500, new { message = "Failed to get attachments" });
        }
    }

    /// <summary>
    /// Uploads a new attachment for an expense.
    /// </summary>
    /// <param name="expenseId">The unique identifier of the expense.</param>
    /// <param name="file">The file to upload.</param>
    /// <param name="userId">Optional user ID of the uploader.</param>
    /// <returns>The created attachment metadata.</returns>
    [HttpPost]
    [DisableRequestSizeLimit]
    public async Task<ActionResult<ExpenseAttachmentDto>> UploadAttachment(
        Guid expenseId,
        [FromForm] IFormFile file,
        [FromForm] string? userId)
    {
        try
        {
            _logger.LogInformation("Uploading attachment for expense {ExpenseId}", expenseId);

            var expense = await _context.Expenses.FindAsync(expenseId);
            if (expense == null)
            {
                _logger.LogWarning("Expense not found for attachment upload: {ExpenseId}", expenseId);
                return NotFound(new { message = "Expense not found" });
            }

            if (file == null || file.Length == 0)
            {
                _logger.LogWarning("No file provided for upload");
                return BadRequest(new { message = "No file provided" });
            }

            if (file.Length > _maxFileSizeBytes)
            {
                var maxSizeMB = _maxFileSizeBytes / (1024 * 1024);
                _logger.LogWarning("File size exceeds limit: {FileSize} bytes", file.Length);
                return BadRequest(new { message = $"File size exceeds {maxSizeMB} MB limit" });
            }

            if (!_allowedFileTypes.Contains(file.ContentType.ToLower()))
            {
                _logger.LogWarning("Invalid file type: {ContentType}", file.ContentType);
                return BadRequest(new { message = "Invalid file type. Only images (JPEG, PNG, GIF) are allowed." });
            }

            var fileExtension = Path.GetExtension(file.FileName);
            var storedFileName = $"{Guid.NewGuid()}{fileExtension}";
            var filePath = Path.Combine(_uploadPath, storedFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var attachment = new ExpenseAttachment
            {
                Id = Guid.NewGuid(),
                ExpenseId = expenseId,
                OriginalFileName = file.FileName,
                StoredFileName = storedFileName,
                DataType = file.ContentType,
                FileSize = file.Length,
                UploadedByUserId = userId ?? "system",
                UploadedAt = DateTime.UtcNow
            };

            _context.ExpenseAttachments.Add(attachment);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Attachment uploaded for expense {ExpenseId}: {FileName}", expenseId, file.FileName);

            var dto = new ExpenseAttachmentDto
            {
                Id = attachment.Id,
                ExpenseId = attachment.ExpenseId,
                OriginalFileName = attachment.OriginalFileName,
                DataType = attachment.DataType,
                FileSize = attachment.FileSize,
                UploadedByUserId = attachment.UploadedByUserId,
                UploadedAt = attachment.UploadedAt
            };

            return CreatedAtAction(nameof(DownloadAttachment), new { expenseId, id = attachment.Id }, dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload attachment for expense {ExpenseId}", expenseId);
            return StatusCode(500, new { message = "Failed to upload attachment" });
        }
    }

    /// <summary>
    /// Retrieves attachment metadata with base64 encoded data for preview.
    /// </summary>
    /// <param name="expenseId">The unique identifier of the expense.</param>
    /// <param name="id">The unique identifier of the attachment.</param>
    /// <returns>Attachment metadata with base64 data.</returns>
    [HttpGet("{id}/preview")]
    public async Task<ActionResult<ExpenseAttachmentPreviewDto>> GetAttachmentPreview(Guid expenseId, Guid id)
    {
        try
        {
            _logger.LogInformation("Fetching attachment preview {AttachmentId} for expense {ExpenseId}", id, expenseId);

            var attachment = await _context.ExpenseAttachments
                .FirstOrDefaultAsync(a => a.Id == id && a.ExpenseId == expenseId);

            if (attachment == null)
            {
                _logger.LogWarning("Attachment not found: {AttachmentId}", id);
                return NotFound(new { message = "Attachment not found" });
            }

            var preview = new ExpenseAttachmentPreviewDto
            {
                Id = attachment.Id,
                ExpenseId = attachment.ExpenseId,
                OriginalFileName = attachment.OriginalFileName,
                DataType = attachment.DataType,
                FileSize = attachment.FileSize,
                Base64Data = attachment.Base64Data,
                UploadedByUserId = attachment.UploadedByUserId,
                UploadedAt = attachment.UploadedAt
            };

            return Ok(preview);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get attachment preview {AttachmentId}", id);
            return StatusCode(500, new { message = "Failed to get attachment preview" });
        }
    }

    /// <summary>
    /// Downloads an attachment file.
    /// </summary>
    /// <param name="expenseId">The unique identifier of the expense.</param>
    /// <param name="id">The unique identifier of the attachment.</param>
    /// <returns>The file content for download.</returns>
    [HttpGet("{id}")]
    public async Task<IActionResult> DownloadAttachment(Guid expenseId, Guid id)
    {
        try
        {
            _logger.LogInformation("Downloading attachment {AttachmentId} for expense {ExpenseId}", id, expenseId);

            var attachment = await _context.ExpenseAttachments
                .FirstOrDefaultAsync(a => a.Id == id && a.ExpenseId == expenseId);

            if (attachment == null)
            {
                _logger.LogWarning("Attachment not found for download: {AttachmentId}", id);
                return NotFound(new { message = "Attachment not found" });
            }

            if (!string.IsNullOrEmpty(attachment.Base64Data))
            {
                var fileBytes = Convert.FromBase64String(attachment.Base64Data);
                return File(fileBytes, attachment.DataType, attachment.OriginalFileName);
            }

            var filePath = Path.Combine(_uploadPath, attachment.StoredFileName);

            if (!System.IO.File.Exists(filePath))
            {
                _logger.LogWarning("File not found on disk: {FilePath}", filePath);
                return NotFound(new { message = "File not found" });
            }

            var fileBytes2 = await System.IO.File.ReadAllBytesAsync(filePath);
            return File(fileBytes2, attachment.DataType, attachment.OriginalFileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to download attachment {AttachmentId}", id);
            return StatusCode(500, new { message = "Failed to download attachment" });
        }
    }

    /// <summary>
    /// Permanently deletes an attachment from the expense.
    /// </summary>
    /// <param name="expenseId">The unique identifier of the expense.</param>
    /// <param name="id">The unique identifier of the attachment to delete.</param>
    /// <returns>NoContent on success, or error response.</returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAttachment(Guid expenseId, Guid id)
    {
        try
        {
            _logger.LogInformation("Deleting attachment {AttachmentId} from expense {ExpenseId}", id, expenseId);

            var attachment = await _context.ExpenseAttachments
                .FirstOrDefaultAsync(a => a.Id == id && a.ExpenseId == expenseId);

            if (attachment == null)
            {
                _logger.LogWarning("Attachment not found for deletion: {AttachmentId}", id);
                return NotFound(new { message = "Attachment not found" });
            }

            var filePath = Path.Combine(_uploadPath, attachment.StoredFileName);
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }

            _context.ExpenseAttachments.Remove(attachment);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Attachment deleted: {AttachmentId}", id);

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete attachment {AttachmentId}", id);
            return StatusCode(500, new { message = "Failed to delete attachment" });
        }
    }
}

public class ExpenseAttachmentDto
{
    public Guid Id { get; set; }
    public Guid ExpenseId { get; set; }
    public string OriginalFileName { get; set; } = string.Empty;
    public string DataType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string UploadedByUserId { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }
}

public class ExpenseAttachmentPreviewDto : ExpenseAttachmentDto
{
    public string Base64Data { get; set; } = string.Empty;
}
