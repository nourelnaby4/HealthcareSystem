namespace Healthcare.Shared.Kernel.Results;

/// <summary>
/// Result type for command/query outcomes that produce a value. Carries an <see cref="Error"/>
/// on failure. Construct via <see cref="Success{T}"/> / <see cref="Failure"/>.
/// </summary>
/// <typeparam name="T">The success value type.</typeparam>
public readonly record struct Result<T>
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

    /// <summary>Projects the success value; failure propagates unchanged.</summary>
    public Result<TOut> Map<TOut>(Func<T, TOut> mapper)
        => IsFailure ? Result<TOut>.Failure(Error!) : Result<TOut>.Success(mapper(Value!));

    /// <summary>Binds into another result; failure propagates unchanged.</summary>
    public Result<TOut> Bind<TOut>(Func<T, Result<TOut>> next)
        => IsFailure ? Result<TOut>.Failure(Error!) : next(Value!);

    public T GetValueOrThrow()
        => IsSuccess ? Value! : throw new ResultFailureException(Error!);

    public static implicit operator Result<T>(T value) => Success(value);
}
