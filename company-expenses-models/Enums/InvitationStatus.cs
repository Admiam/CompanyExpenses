namespace CompanyExpenses.Models.Enums;

/// <summary>
/// Status of an invitation
/// </summary>
public enum InvitationStatus : byte
{
    /// <summary>Invitation is pending</summary>
    Pending = 0,
    
    /// <summary>Invitation was accepted</summary>
    Accepted = 1,
    
    /// <summary>Invitation expired</summary>
    Expired = 2,
    
    /// <summary>Invitation was cancelled</summary>
    Cancelled = 3
}
