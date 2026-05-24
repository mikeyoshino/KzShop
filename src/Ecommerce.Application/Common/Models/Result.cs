namespace Ecommerce.Application.Common.Models;

public class Result
{
    protected Result(bool isSuccess, BusinessErrorCode? errorCode, string? errorMessage)
    {
        IsSuccess = isSuccess;
        ErrorCode = errorCode;
        ErrorMessage = errorMessage;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public BusinessErrorCode? ErrorCode { get; }
    public string? ErrorMessage { get; }

    public static Result Success()
    {
        return new Result(true, null, null);
    }

    public static Result Failure(BusinessErrorCode errorCode, string errorMessage)
    {
        return new Result(false, errorCode, errorMessage);
    }

    public static Result<T> Success<T>(T value)
    {
        return Result<T>.Success(value);
    }

    public static Result<T> Failure<T>(BusinessErrorCode errorCode, string errorMessage)
    {
        return Result<T>.Failure(errorCode, errorMessage);
    }
}

public sealed class Result<T> : Result
{
    private Result(bool isSuccess, T? value, BusinessErrorCode? errorCode, string? errorMessage)
        : base(isSuccess, errorCode, errorMessage)
    {
        Value = value;
    }

    public T? Value { get; }

    public static Result<T> Success(T value)
    {
        return new Result<T>(true, value, null, null);
    }

    public static new Result<T> Failure(BusinessErrorCode errorCode, string errorMessage)
    {
        return new Result<T>(false, default, errorCode, errorMessage);
    }
}
