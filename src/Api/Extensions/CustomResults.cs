using Microsoft.AspNetCore.Http; // Required for IResult, Results, and StatusCodes
using SharedCore; // Assumed to contain the Result, Error, and ValidationError types

namespace Api.Extensions;

/// <summary>
/// Provides extension methods for converting custom <see cref="Result"/> objects
/// into standardized <see cref="IResult"/> HTTP responses, following the RFC 7807 Problem Details specification.
/// </summary>
public static class CustomResults
{
    /// <summary>
    /// Converts a failed <see cref="Result"/> object into a standardized HTTP Problem Details response (RFC 7807).
    /// This is an extension method on the <see cref="Result"/> type.
    /// </summary>
    /// <param name="result">The result object from a service or application layer, which is expected to have failed.</param>
    /// <returns>An <see cref="IResult"/> that generates an appropriate error response (e.g., 400, 404, 409, 500) with a Problem Details body.</returns>
    /// <exception cref="InvalidOperationException">Thrown if this method is called on a successful result, as it indicates a developer error.</exception>
    public static IResult Problem(this Result result)
    {
        // Guard clause: This method should only ever be called on a failed result.
        // If it's called on a successful one, it's a bug in the code that needs to be fixed.
        if (result.IsSuccess)
        {
            throw new InvalidOperationException("Cannot convert a successful result to a problem.");
        }

        // Use the built-in Results.Problem to create a standard ProblemDetails response.
        // The details of the response are determined by a series of local static functions
        // that map the custom Error object to standard ProblemDetails fields.
        return Results.Problem(
            title: GetTitle(result.Error),
            detail: GetDetail(result.Error),
            type: GetType(result.Error.Type),
            statusCode: GetStatusCode(result.Error.Type),
            extensions: GetErrors(result));

        // --- Local Static Functions for Mapping ---

        /// <summary>
        /// Maps the Error.Code to the ProblemDetails Title for specific, known error types.
        /// For unknown errors, it provides a generic "Server failure" title.
        /// </summary>
        static string GetTitle(Error error) =>
            error.Type switch
            {
                ErrorType.Validation => error.Code, // e.g., "Validation.Failure"
                ErrorType.Problem => error.Code,
                ErrorType.NotFound => error.Code,   // e.g., "Quote.NotFound"
                ErrorType.Conflict => error.Code,   // e.g., "Email.AlreadyExists"
                _ => "Server failure"
            };

        /// <summary>
        /// Maps the Error.Description to the ProblemDetails Detail for specific, known error types.
        /// For unknown errors, it provides a generic error description.
        /// </summary>
        static string GetDetail(Error error) =>
            error.Type switch
            {
                ErrorType.Validation => error.Description, // e.g., "One or more validation errors occurred."
                ErrorType.Problem => error.Description,
                ErrorType.NotFound => error.Description,   // e.g., "A quote with the specified ID was not found."
                ErrorType.Conflict => error.Description,
                _ => "An unexpected error occurred"
            };

        /// <summary>
        /// Provides a standard URI reference for the error type, pointing to the relevant RFC 7231 section that defines the status code.
        /// This makes the API response more machine-readable and self-descriptive.
        /// </summary>
        static string GetType(ErrorType errorType) =>
            errorType switch
            {
                // 400 Bad Request
                ErrorType.Validation => "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                ErrorType.Problem => "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                // 404 Not Found
                ErrorType.NotFound => "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                // 409 Conflict
                ErrorType.Conflict => "https://tools.ietf.org/html/rfc7231#section-6.5.8",
                // 500 Internal Server Error (default case)
                _ => "https://tools.ietf.org/html/rfc7231#section-6.6.1"
            };
        
        /// <summary>
        /// Maps the custom ErrorType enum to a standard HTTP status code.
        /// Uses constants from StatusCodes for clarity and correctness.
        /// </summary>
        static int GetStatusCode(ErrorType errorType) =>
            errorType switch
            {
                // Using 'or' pattern for concise mapping
                ErrorType.Validation or ErrorType.Problem => StatusCodes.Status400BadRequest,
                ErrorType.NotFound => StatusCodes.Status404NotFound,
                ErrorType.Conflict => StatusCodes.Status409Conflict,
                _ => StatusCodes.Status500InternalServerError
            };

        /// <summary>
        /// Extracts validation errors to be included in the 'extensions' member of the Problem Details object.
        /// This is only applicable for validation-specific errors.
        /// </summary>
        /// <param name="result">The result object.</param>
        /// <returns>A dictionary containing the validation errors if the error is a <see cref="ValidationError"/>; otherwise, null.</returns>
        static Dictionary<string, object?>? GetErrors(Result result)
        {
            // Check if the error is specifically a ValidationError.
            // This is assumed to be a special type of Error that contains a list of individual field errors.
            if (result.Error is not ValidationError validationError)
            {
                // If it's not a validation error, there are no extra details to add.
                return null;
            }
            
            // If it is a validation error, create a dictionary to hold the structured error details.
            // This is a standard way to represent validation failures in a Problem Details response,
            // by adding an "errors" member to the JSON object.
            return new Dictionary<string, object?>
            {
                { "errors", validationError.Errors }
            };
        }
    }
}