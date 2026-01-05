using System.Text.Json.Serialization;

namespace CompanyExpenses.Api.DTOs;

/// <summary>
/// DTO for creating a new expense with attachments
/// </summary>
public class CreateExpenseDto
{
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "CZK";
    public DateOnly ExpenseDate { get; set; }
    public Guid CategoryId { get; set; }
    public Guid WorkplaceId { get; set; }
    public List<ExpenseAttachmentUploadDto> Attachments { get; set; } = new();
}

/// <summary>
/// DTO for uploading expense attachment with base64 data
/// </summary>
public class ExpenseAttachmentUploadDto
{
    [JsonPropertyName("fileName")]
    public string FileName { get; set; } = string.Empty;

    [JsonPropertyName("fileType")]
    public string FileType { get; set; } = string.Empty;

    [JsonPropertyName("base64Data")]
    public string Base64Data { get; set; } = string.Empty;

    [JsonPropertyName("originalFileSize")]
    public long OriginalFileSize { get; set; }
}

/// <summary>
/// DTO for returning expense attachment data
/// </summary>
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

/// <summary>
/// DTO for approval/rejection requests
/// </summary>
public class ApprovalRequest
{
    public string? Note { get; set; }
}

public class UpdateAmountRequest
{
    public decimal Amount { get; set; }
}

public class UpdateCategoryRequest
{
    public Guid CategoryId { get; set; }
}

public class UpdateAttachmentsRequest
{
    public List<ExpenseAttachmentUploadDto> Attachments { get; set; } = new();
}
