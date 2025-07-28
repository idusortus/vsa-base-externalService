namespace SharedCore;

/// <summary>
/// Defines the different semantic types of an error.
/// This provides a structured way to categorize errors, which can be used by higher-level layers (like the API)
/// to make decisions, such as mapping an error type to a specific HTTP status code.
/// For example, ErrorType.NotFound would map to a 404 Not Found response.
/// </summary>
public enum ErrorType
{
    /// <summary>
    /// A general failure. This is the default and maps to a 500 Internal Server Error or 400 Bad Request.
    /// </summary>
    Failure = 0,
    /// <summary>
    /// A failure due to invalid input data. Maps to a 400 Bad Request.
    /// </summary>
    Validation = 1,
    /// <summary>
    /// An unexpected or unrecoverable error. Maps to a 500 Internal Server Error.
    /// </summary>
    Problem = 2,
    /// <summary>
    /// A resource was not found. Maps to a 404 Not Found.
    /// </summary>
    NotFound = 3,
    /// <summary>
    /// The request could not be completed due to a conflict with the current state of the resource. Maps to a 409 Conflict.
    /// </summary>
    Conflict = 4
}

/// <summary>
/// Represents a structured, immutable error object, used as part of the Result pattern.
/// It contains a machine-readable code, a human-readable description, and a semantic type.
/// </summary>
public record Error
{
    /// <summary>
    /// Represents the absence of an error. This is the sentinel value for a successful Result.
    /// Its 'Code' is empty, signifying that no error occurred.
    /// </summary>
    public static readonly Error None = new(string.Empty, string.Empty, ErrorType.Failure);
    
    /// <summary>
    /// A common, reusable error representing that an unexpected null value was provided where a value was required.
    /// </summary>
    public static readonly Error NullValue = new(
        "General.NullValue", // A stable, machine-readable code
        "A null value was provided where it was not allowed.",
        ErrorType.Failure);

    /// <summary>
    /// Initializes a new instance of the Error record.
    /// Using the static factory methods (e.g., Error.Failure, Error.NotFound) is the preferred way to create errors.
    /// </summary>
    /// <param name="code">The unique, machine-readable code for this error (e.g., "Orders.NotFound").</param>
    /// <param name="description">The human-readable description of the error.</param>
    /// <param name="type">The semantic type of the error.</param>
    public Error(string code, string description, ErrorType type)
    {
        Code = code;
        Description = description;
        Type = type;
    }

    /// <summary>
    /// Gets the unique, machine-readable error code. This can be used for programmatic error handling or localization.
    /// </summary>
    public string Code { get; }

    /// <summary>
    /// Gets the human-readable description of the error, suitable for logging or debugging.
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// Gets the semantic type of the error, used for categorization and mapping to outcomes like HTTP status codes.
    /// </summary>
    public ErrorType Type { get; }

    /// <summary>
    /// Creates a new Error object representing a general failure.
    /// </summary>
    /// <param name="code">The unique error code.</param>
    /// <param name="description">The error description.</param>
    /// <returns>A new Error instance with type Failure.</returns>
    public static Error Failure(string code, string description) =>
        new(code, description, ErrorType.Failure);

    /// <summary>
    /// Creates a new Error object representing a "Not Found" condition.
    /// </summary>
    /// <param name="code">The unique error code.</param>
    /// <param name="description">The error description.</param>
    /// <returns>A new Error instance with type NotFound.</returns>
    public static Error NotFound(string code, string description) =>
        new(code, description, ErrorType.NotFound);

    /// <summary>
    /// Creates a new Error object representing an unexpected server problem.
    /// </summary>
    /// <param name="code">The unique error code.</param>
    /// <param name="description">The error description.</param>
    /// <returns>A new Error instance with type Problem.</returns>
    public static Error Problem(string code, string description) =>
        new(code, description, ErrorType.Problem);

    /// <summary>
    /// Creates a new Error object representing a state conflict.
    /// </summary>
    /// <param name="code">The unique error code.</param>
    /// <param name="description">The error description.</param>
    /// <returns>A new Error instance with type Conflict.</returns>
    public static Error Conflict(string code, string description) =>
        new(code, description, ErrorType.Conflict);
}