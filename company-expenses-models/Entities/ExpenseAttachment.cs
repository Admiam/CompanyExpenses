namespace CompanyExpenses.Models.Entities;

public class ExpenseAttachment
{
    public Guid Id { get; set; }
    public Guid ExpenseId { get; set; }
    public string OriginalFileName { get; set; } = string.Empty;
    public string StoredFileName { get; set; } = string.Empty;
    public string DataType { get; set; } = string.Empty; // "image/jpeg", "application/pdf"...
    public long FileSize { get; set; }
    public string UploadedByUserId { get; set; } = string.Empty; // AspNetUsers.Id
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Expense? Expense { get; set; }
}
