namespace CompanyExpenses.Models.Enums;

/// <summary>
/// Status of an expense request
/// </summary>
public enum ExpenseStatus : byte
{
    /// <summary>Pending approval</summary>
    Pending = 0,
    
    /// <summary>Approved by manager</summary>
    Approved = 1,
    
    /// <summary>Rejected by manager</summary>
    Rejected = 2,
    
    /// <summary>Paid/reimbursed</summary>
    Paid = 3
}
