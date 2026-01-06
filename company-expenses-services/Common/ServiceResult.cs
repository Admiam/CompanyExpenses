namespace CompanyExpenses.Services.Common;

/// <summary>
/// Service result wrapper for consistent return values
/// </summary>
/// <typeparam name="T">Data type</typeparam>
public class ServiceResult<T>
{
    public bool IsSuccess { get; private set; }
    public T? Data { get; private set; }
    public string? ErrorMessage { get; private set; }
    public ServiceErrorType ErrorType { get; private set; }

    private ServiceResult() { }

    public static ServiceResult<T> Success(T data) => new()
    {
        IsSuccess = true,
        Data = data
    };

    public static ServiceResult<T> NotFound(string message = "Resource not found") => new()
    {
        IsSuccess = false,
        ErrorMessage = message,
        ErrorType = ServiceErrorType.NotFound
    };

    public static ServiceResult<T> BadRequest(string message) => new()
    {
        IsSuccess = false,
        ErrorMessage = message,
        ErrorType = ServiceErrorType.BadRequest
    };

    public static ServiceResult<T> Unauthorized(string message = "User not authenticated") => new()
    {
        IsSuccess = false,
        ErrorMessage = message,
        ErrorType = ServiceErrorType.Unauthorized
    };

    public static ServiceResult<T> Error(string message) => new()
    {
        IsSuccess = false,
        ErrorMessage = message,
        ErrorType = ServiceErrorType.InternalError
    };
}

/// <summary>
/// Service result without data
/// </summary>
public class ServiceResult
{
    public bool IsSuccess { get; private set; }
    public string? ErrorMessage { get; private set; }
    public ServiceErrorType ErrorType { get; private set; }

    private ServiceResult() { }

    public static ServiceResult Success() => new() { IsSuccess = true };

    public static ServiceResult NotFound(string message = "Resource not found") => new()
    {
        IsSuccess = false,
        ErrorMessage = message,
        ErrorType = ServiceErrorType.NotFound
    };

    public static ServiceResult BadRequest(string message) => new()
    {
        IsSuccess = false,
        ErrorMessage = message,
        ErrorType = ServiceErrorType.BadRequest
    };

    public static ServiceResult Unauthorized(string message = "User not authenticated") => new()
    {
        IsSuccess = false,
        ErrorMessage = message,
        ErrorType = ServiceErrorType.Unauthorized
    };

    public static ServiceResult Error(string message) => new()
    {
        IsSuccess = false,
        ErrorMessage = message,
        ErrorType = ServiceErrorType.InternalError
    };
}

public enum ServiceErrorType
{
    None,
    NotFound,
    BadRequest,
    Unauthorized,
    InternalError
}
