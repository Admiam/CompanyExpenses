namespace CompanyExpenses.Models.Enums;

/// <summary>
/// Action taken on an expense approval
/// </summary>
public enum ApprovalAction : byte
{
    /// <summary>Expense was approved</summary>
    Approved = 1,
    
    /// <summary>Expense was rejected</summary>
    Rejected = 2,
    
    /// <summary>Expense was returned for revision</summary>
    ReturnedForRevision = 3
}
