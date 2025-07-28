namespace SharedCore;

/// <summary>
/// Represents a specialized error that encapsulates a collection of individual validation failures.
/// This is a "Composite Error"—it is an Error itself, but it also contains other Error objects.
/// It's used when a single operation (like validating a request object) can result in multiple distinct validation errors.
/// </summary>
public sealed record ValidationError : Error
{
    /// <summary>
    /// Initializes a new instance of the ValidationError.
    /// It calls the base Error constructor with a generic "Validation.General" code, as its primary purpose
    /// is to act as a container for more specific, detailed errors.
    /// </summary>
    /// <param name="errors">An array of the specific validation errors that were found.</param>
    public ValidationError(Error[] errors)
        : base(
            "Validation.General", // A stable, machine-readable code for "a validation error occurred".
            "One or more validation errors occurred",
            ErrorType.Validation) // Sets the semantic type to Validation.
    {
        Errors = errors;
    }

    /// <summary>
    /// Gets the array of detailed, underlying validation errors.
    /// This allows the consumer (like the API layer) to inspect each individual validation failure.
    /// </summary>
    public Error[] Errors { get; }

    /// <summary>
    /// A convenient factory method to create a ValidationError from a collection of Result objects.
    /// It automatically filters the collection to find all failed results and aggregates their
    /// individual Error objects into a new ValidationError.
    /// </summary>
    /// <param name="results">An enumeration of Result objects, typically from validating multiple properties or rules.</param>
    /// <returns>A new ValidationError containing the errors from all failed results.</returns>
    public static ValidationError FromResults(IEnumerable<Result> results) =>
        // This LINQ chain does the following:
        // 1. Where(r => r.IsFailure): Filters the incoming list to keep only the failed Result objects.
        // 2. Select(r => r.Error): For each of those failed results, it extracts the Error object.
        // 3. ToArray(): It converts the resulting collection of Error objects into an array.
        // 4. new(...): Finally, it creates a new ValidationError instance using this array of errors.
        new(results.Where(r => r.IsFailure).Select(r => r.Error).ToArray());
}