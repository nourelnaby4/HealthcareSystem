namespace Healthcare.Shared.Kernel.Results;

/// <summary>
/// Thrown when code accesses <see cref="Result{T}.GetValueOrThrow"/> on a failed result.
/// Should never reach an HTTP response — handlers translate <see cref="Error"/> into safe
/// ProblemDetails before this is reached.
/// </summary>
public sealed class ResultFailureException(Error error) : InvalidOperationException(error.Message)
{
    public Error Error { get; } = error;
}
