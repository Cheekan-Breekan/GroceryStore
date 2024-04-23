namespace GroceryStore.Core.ResultModels;
public class Result<T>
{
    public Error Error { get; }
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public T? Value { get; }
    private Result(Error error, bool isSuccess, T? value = default)
    {
        if (Error.None == error && !isSuccess)
        {
            throw new ArgumentException("Cannot create a failure result with Error.None", nameof(error));
        }
        Error = error;
        IsSuccess = isSuccess;
        Value = value;
    }
    public static Result<T> Success(T value) => new(Error.None, true, value);
    public static Result<T> Failure(Error error) => new(error, false);
}
public class Result
{
    public Error Error { get; }
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    private Result(Error error, bool isSuccess)
    {
        if (Error.None == error && !isSuccess)
        {
            throw new ArgumentException("Cannot create a failure result with Error.None", nameof(error));
        }
        Error = error;
        IsSuccess = isSuccess;
    }
    public static Result Success() => new(Error.None, true);
    public static Result Failure(Error error) => new(error, false);
}
