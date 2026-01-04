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
    public string OriginalFileName { get; set; } = string.Empty;
    public string DataType { get; set; } = string.Empty;
    public string Base64Data { get; set; } = string.Empty; // Compressed image in base64
    public long OriginalFileSize { get; set; } // Original file size before compression
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
