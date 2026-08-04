namespace Healthcare.Shared.Kernel.Results;

/// </summary>
/// <typeparam name="T">The success value type.</typeparam>
public sealed class Result<T>
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public T? Value { get; }
    public Error? Error { get; }

    private Result(bool isSuccess, T? value, Error? error)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
    }

    public static Result<T> Success(T value) => new(true, value, null);

    public static Result<T> Failure(Error error) => new(false, default, error);

    public static Result<T> Failure(ErrorType type, string code, string message)
        => new(false, default, new Error(type, code, message));

    public T GetValueOrThrow()
        => IsSuccess ? Value! : throw new ResultFailureException(Error!);

    public static implicit operator Result<T>(T value) => Success(value);
}
