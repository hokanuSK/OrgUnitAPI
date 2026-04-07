namespace CompanyStructure.Api.Application.Common;

public sealed class AppException(string message, int statusCode, string? errorCode = null) : Exception(message)
{
    public int StatusCode { get; } = statusCode;
    public string? ErrorCode { get; } = errorCode;

    public static AppException BadRequest(string message, string? errorCode = null)
        => new(message, StatusCodes.Status400BadRequest, errorCode);

    public static AppException NotFound(string message, string? errorCode = null)
        => new(message, StatusCodes.Status404NotFound, errorCode);

    public static AppException Conflict(string message, string? errorCode = null)
        => new(message, StatusCodes.Status409Conflict, errorCode);

    public static AppException Unprocessable(string message, string? errorCode = null)
        => new(message, StatusCodes.Status422UnprocessableEntity, errorCode);
}
