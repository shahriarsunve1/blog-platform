namespace BlogAPI.Core.DTOs;

/// <summary>
/// Standard API response wrapper
/// </summary>
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public int StatusCode { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public List<ErrorDetail> Errors { get; set; } = new();

    public static ApiResponse<T> Ok(T data, string message = "", int statusCode = 200) => new()
    {
        Success = true,
        StatusCode = statusCode,
        Message = message,
        Data = data
    };

    public static ApiResponse<T> Fail(string message, int statusCode) => new()
    {
        Success = false,
        StatusCode = statusCode,
        Message = message
    };
}

/// <summary>
/// Error detail in response
/// </summary>
public class ErrorDetail
{
    public string Field { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// Paginated response
/// </summary>
public class PaginatedResponse<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}
