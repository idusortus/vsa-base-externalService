using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Api.Extensions;

/// <summary>
/// Implements the IExceptionHandler interface to provide a centralized mechanism for handling all unhandled exceptions.
/// Its primary responsibility is to catch any exception that isn't explicitly handled elsewhere, log it,
/// and return a standardized, user-friendly error response conforming to the ProblemDetails standard (RFC 7807).
/// This prevents the application from crashing and leaking sensitive stack trace information to the client.
/// </summary>
/// <param name="logger">An ILogger instance injected via dependency injection to record exception details.</param>
internal sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    : IExceptionHandler // This interface is the key to hooking into the .NET 8 exception handling middleware.
{
    /// <summary>
    //  The core method invoked by the exception handling middleware when an unhandled exception is caught.
    /// </summary>
    /// <param name="httpContext">The HttpContext for the current request, allowing access to the Response object.</param>
    /// <param name="exception">The unhandled exception that was caught in the request pipeline.</param>
    /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
    /// <returns>
    /// A ValueTask<bool> indicating if the exception was handled. Returning 'true' signifies that the
    /// exception has been dealt with, a response has been sent, and no further processing should occur.
    /// </returns>
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        // Log the full exception details for debugging and auditing purposes.
        // It's crucial to log the 'exception' object itself, not just the message,
        // to capture the stack trace and other important context.
        logger.LogError(exception, "Unhandled exception occurred: {Message}", exception.Message);

        // Create a new ProblemDetails object. This is a standardized, machine-readable format
        // for specifying errors in HTTP API responses.
        var problemDetails = new ProblemDetails
        {
            // Set the HTTP status code to 500 Internal Server Error, as this is an unexpected failure.
            Status = StatusCodes.Status500InternalServerError,
            
            // Provide a URI that uniquely identifies this type of problem. It points to the RFC
            // specification for the 500 status code, making the error understandable by generic HTTP clients.
            Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.1",
            
            // Provide a short, human-readable summary of the problem. Avoid exposing internal details.
            Title = "Server failure"
        };

        // Set the HTTP status code on the actual response headers.
        httpContext.Response.StatusCode = problemDetails.Status.Value;

        // Serialize the ProblemDetails object to JSON and write it to the HTTP response body.
        // This sends the standardized error message back to the client that made the request.
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        // Return 'true' to signal to the exception handling middleware that we have successfully
        // handled the exception and sent a response. The request pipeline will be short-circuited.
        return true;
    }
}