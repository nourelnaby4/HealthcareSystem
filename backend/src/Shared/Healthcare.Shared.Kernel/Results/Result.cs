namespace Healthcare.Shared.Kernel.Results;

/// <summary>
/// Result type for command/query outcomes that produce no value. Carries an <see cref="Error"/>
/// on failure. Construct via <see cref="Success"/> / <see cref="Failure(Healthcare.Shared.Kernel.Results.Error)"/>.
/// </summary>
public readonly record struct Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error? Error { get; }

    private Result(bool isSuccess, Error? error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    public static Result Success() => new(true, null);

    public static Result Failure(Error error) => new(false, error);

    public static Result Failure(ErrorType type, string code, string message)
        => new(false, new Error(type, code, message));

    public Result<T> ToResult<T>() => IsSuccess
        ? throw new InvalidOperationException("Cannot convert a valueless success to a value-carrying result.")
        : Result<T>.Failure(Error!);
}
