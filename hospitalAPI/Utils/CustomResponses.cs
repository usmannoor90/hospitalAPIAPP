// File: Utils/ApiResponse.cs
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace hospitalAPI.Utils;

public class CustomResponses<T>
{
    public bool IsSuccess { get; set; }
    public T? Data { get; set; }
    public string? Message { get; set; }
    public string? ErrorCode { get; set; }
    public List<string> Errors { get; set; } = new();

    // Helper: Success response
    public static CustomResponses<T> Success(T data, string message = "Operation successful.")
    {
        return new CustomResponses<T>
        {
            IsSuccess = true,
            Data = data,
            Message = message,
            Errors = new List<string>()
        };
    }

    // Helper: Failure response
    public static CustomResponses<T> Failure(string message, string errorCode = "", List<string>? errors = null)
    {
        return new CustomResponses<T>
        {
            IsSuccess = false,
            Message = message,
            ErrorCode = errorCode,
            Errors = errors ?? new List<string>()
        };
    }

    // Optional: For validation errors
    public static CustomResponses<T> ValidationError(ModelStateDictionary modelState, string message = "Validation failed.")
    {
        var errors = modelState.Values
            .SelectMany(v => v.Errors)
            .Select(e => e.ErrorMessage)
            .ToList();

        return Failure(message, "VALIDATION_ERROR", errors);
    }
}