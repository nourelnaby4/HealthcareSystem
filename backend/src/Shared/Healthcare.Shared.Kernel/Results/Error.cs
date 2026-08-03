namespace Healthcare.Shared.Kernel.Results;

/// <summary>
/// Categorizes a domain/application error so handlers can translate it into the correct
/// HTTP status code without leaking internal detail.
/// </summary>
public enum ErrorType
{
    /// <summary>Resource does not exist (HTTP 404).</summary>
    NotFound,

    /// <summary>A uniqueness/concurrency invariant was violated (HTTP 409).</summary>
    Conflict,

    /// <summary>Input failed business validation rules (HTTP 422).</summary>
    Validation,

    /// <summary>Business rule/prevent action (HTTP 422).</summary>
    BusinessRule,

    /// <summary>The caller is not permitted to perform the action (HTTP 403).</summary>
    Forbidden,

    /// <summary>An unexpected failure occurred (HTTP 500).</summary>
    Failure,
}

/// <summary>
/// A structured, code-stable domain error. Codes are stable identifiers suitable for problem-details
/// <c>errors</c> maps; <see cref="Message"/> is a safe, user-facing string.
/// </summary>
public sealed record Error(ErrorType Type, string Code, string Message)
{
    public static Error NotFound(string code, string message) => new(ErrorType.NotFound, code, message);
    public static Error Conflict(string code, string message) => new(ErrorType.Conflict, code, message);
    public static Error Validation(string code, string message) => new(ErrorType.Validation, code, message);
    public static Error BusinessRule(string code, string message) => new(ErrorType.BusinessRule, code, message);
    public static Error Forbidden(string code, string message) => new(ErrorType.Forbidden, code, message);
    public static Error Failure(string code, string message) => new(ErrorType.Failure, code, message);
}
