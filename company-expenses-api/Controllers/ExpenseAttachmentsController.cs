using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CompanyExpenses.Database.Data;
using CompanyExpenses.Models.Entities;

namespace CompanyExpenses.Api.Controllers;

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

        // Configure upload path
        _uploadPath = _configuration["FileStorage:UploadPath"]
            ?? Path.Combine(Directory.GetCurrentDirectory(), "uploads", "receipts");

        // Configure file size limit (default 10 MB)
        _maxFileSizeBytes = _configuration.GetValue<long>("FileStorage:MaxFileSizeBytes", 10_485_760);

        // Configure allowed file types
        var allowedTypesConfig = _configuration["FileStorage:AllowedFileTypes"]
            ?? "image/jpeg,image/jpg,image/png,image/gif";
        _allowedFileTypes = allowedTypesConfig.Split(',', StringSplitOptions.RemoveEmptyEntries);

        // Ensure directory exists
        Directory.CreateDirectory(_uploadPath);
    }

    /// <summary>
    /// Get all attachments for an expense
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ExpenseAttachmentDto>>> GetAttachments(Guid expenseId)
    {
        try
        {
            var expense = await _context.Expenses
                .Include(e => e.Attachments)
                .FirstOrDefaultAsync(e => e.Id == expenseId);

            if (expense == null)
            {
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
    /// Upload attachment for an expense
    /// </summary>
    [HttpPost]
    [DisableRequestSizeLimit] // Limit is checked programmatically based on configuration
    public async Task<ActionResult<ExpenseAttachmentDto>> UploadAttachment(
        Guid expenseId,
        [FromForm] IFormFile file,
        [FromForm] string? userId)
    {
        try
        {
            // Validate expense exists
            var expense = await _context.Expenses.FindAsync(expenseId);
            if (expense == null)
            {
                return NotFound(new { message = "Expense not found" });
            }

            // Validate file
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { message = "No file provided" });
            }

            // Validate file size using configured limit
            if (file.Length > _maxFileSizeBytes)
            {
                var maxSizeMB = _maxFileSizeBytes / (1024 * 1024);
                return BadRequest(new { message = $"File size exceeds {maxSizeMB} MB limit" });
            }

            // Validate file type using configured allowed types
            if (!_allowedFileTypes.Contains(file.ContentType.ToLower()))
            {
                return BadRequest(new { message = "Invalid file type. Only images (JPEG, PNG, GIF) are allowed." });
            }

            // Generate unique filename
            var fileExtension = Path.GetExtension(file.FileName);
            var storedFileName = $"{Guid.NewGuid()}{fileExtension}";
            var filePath = Path.Combine(_uploadPath, storedFileName);

            // Save file to disk
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Create attachment record
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
    /// Get attachment with base64 data for preview
    /// </summary>
    [HttpGet("{id}/preview")]
    public async Task<ActionResult<ExpenseAttachmentPreviewDto>> GetAttachmentPreview(Guid expenseId, Guid id)
    {
        try
        {
            var attachment = await _context.ExpenseAttachments
                .FirstOrDefaultAsync(a => a.Id == id && a.ExpenseId == expenseId);

            if (attachment == null)
            {
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
    /// Download an attachment
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> DownloadAttachment(Guid expenseId, Guid id)
    {
        try
        {
            var attachment = await _context.ExpenseAttachments
                .FirstOrDefaultAsync(a => a.Id == id && a.ExpenseId == expenseId);

            if (attachment == null)
            {
                return NotFound(new { message = "Attachment not found" });
            }

            // If stored as base64, return decoded data
            if (!string.IsNullOrEmpty(attachment.Base64Data))
            {
                var fileBytes = Convert.FromBase64String(attachment.Base64Data);
                return File(fileBytes, attachment.DataType, attachment.OriginalFileName);
            }

            // Fallback to file system (legacy support)
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
    /// Delete an attachment
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAttachment(Guid expenseId, Guid id)
    {
        try
        {
            var attachment = await _context.ExpenseAttachments
                .FirstOrDefaultAsync(a => a.Id == id && a.ExpenseId == expenseId);

            if (attachment == null)
            {
                return NotFound(new { message = "Attachment not found" });
            }

            // Delete file from disk
            var filePath = Path.Combine(_uploadPath, attachment.StoredFileName);
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }

            // Delete database record
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
