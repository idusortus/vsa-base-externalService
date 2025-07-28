using FluentValidation;
using MediatR;
using SharedCore;

namespace Application.Abstractions.Behaviors;

public class ValidationBehavior<TRequest, TResponse> 
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : Result // Ensure we're only working with our Result type
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
        {
            // If there are no validators, just continue to the handler
            return await next();
        }

        // Run all validators and collect the results
        var context = new ValidationContext<TRequest>(request);

        var validationErrors = _validators
            .Select(validator => validator.Validate(context))
            .SelectMany(validationResult => validationResult.Errors)
            .Where(validationFailure => validationFailure is not null)
            .Select(failure => new Error( // Convert FluentValidation errors to our Error record
                failure.PropertyName,
                failure.ErrorMessage,
                ErrorType.Validation))
            .Distinct()
            .ToArray();

        if (validationErrors.Any())
        {
            // If there are validation errors, short-circuit the pipeline
            // and return a failure result.
            // This uses reflection to call the static Failure<T> method on the Result class.
            return CreateValidationResult(validationErrors);
        }

        // If validation succeeds, continue to the actual request handler
        return await next();
    }

    // This is a bit of reflection magic to create a Failure Result of the correct generic type
    private static TResponse CreateValidationResult(Error[] errors)
    {
        var validationError = new ValidationError(errors);

        if (typeof(TResponse) == typeof(Result))
        {
            return (TResponse)Result.Failure(validationError);
        }

        object validationResult = typeof(Result)
            .GetMethods()
            .First(m => 
                m.Name == nameof(Result.Failure) && 
                m.IsGenericMethod)
            .MakeGenericMethod(typeof(TResponse).GenericTypeArguments[0])
            .Invoke(null, new object?[] { validationError })!;

        return (TResponse)validationResult;
    }
}