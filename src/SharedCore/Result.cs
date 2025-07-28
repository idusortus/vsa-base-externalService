using System.Diagnostics.CodeAnalysis;

namespace SharedCore;

/// <summary>
/// Represents the non-generic base class for an operation's outcome.
/// It encapsulates whether the operation was successful or not and holds an Error object in case of failure.
/// This pattern avoids the use of exceptions for predictable, logical failures.
/// </summary>
public class Result
{
    /// <summary>
    /// The protected constructor that enforces the core rule of the Result pattern:
    /// A successful result cannot have an error, and a failed result MUST have an error.
    /// This prevents the creation of an invalid Result state.
    /// </summary>
    /// <param name="isSuccess">A flag indicating if the operation was successful.</param>
    /// <param name="error">The error object. Must be Error.None for success, and a valid error for failure.</param>
    /// <exception cref="ArgumentException">Thrown if the invariant rule for success/failure and error is violated.</exception>
    protected Result(bool isSuccess, Error error)
    {
        // This is a critical guard clause. It ensures that it's impossible to create
        // an invalid Result object, such as one that is both successful and has an error.
        if (isSuccess && error != Error.None ||
            !isSuccess && error == Error.None)
        {
            throw new ArgumentException("Invalid error state for a result.", nameof(error));
        }

        IsSuccess = isSuccess;
        Error = error;
    }

    /// <summary>
    /// Gets a value indicating whether the operation was successful.
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Gets a value indicating whether the operation failed. This is the convenient inverse of IsSuccess.
    /// </summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// Gets the Error object associated with a failed result.
    /// For a successful result, this will be Error.None.
    /// </summary>
    public Error Error { get; }

    /// <summary>
    /// Creates a successful Result instance without a value.
    /// </summary>
    /// <returns>A successful Result.</returns>
    public static Result Success() => new(true, Error.None);

    /// <summary>
    /// Creates a successful Result<TValue> instance with the provided value.
    /// </summary>
    /// <typeparam name="TValue">The type of the successful value.</typeparam>
    /// <param name="value">The value to be wrapped in the result.</param>
    /// <returns>A successful Result containing the value.</returns>
    public static Result<TValue> Success<TValue>(TValue value) => new(value, true, Error.None);

    /// <summary>
    /// Creates a failed Result instance with the specified error.
    /// </summary>
    /// <param name="error">The error that occurred.</param>
    /// <returns>A failed Result.</returns>
    public static Result Failure(Error error) => new(false, error);

    /// <summary>
    /// Creates a failed Result<TValue> instance with the specified error.
    /// </summary>
    /// <typeparam name="TValue">The type of the value that would have been returned on success.</typeparam>
    /// <param name="error">The error that occurred.</param>
    /// <returns>A failed Result.</returns>
    public static Result<TValue> Failure<TValue>(Error error) => new(default, false, error);
}

/// <summary>
/// Represents the generic version of a Result, encapsulating either a successful value of type TValue or an error.
/// </summary>
/// <typeparam name="TValue">The type of the value returned from a successful operation.</typeparam>
public class Result<TValue> : Result
{
    // The backing field for the success value. It is nullable because a failed result has no value.
    private readonly TValue? _value;

    /// <summary>
    /// Protected constructor for the generic Result.
    /// </summary>
    /// <param name="value">The value for a successful result, or default for a failure.</param>
    /// <param name="isSuccess">A flag indicating if the operation was successful.</param>
    /// <param name="error">The error object for a failed result.</param>
    protected internal Result(TValue? value, bool isSuccess, Error error)
        : base(isSuccess, error)
    {
        _value = value;
    }

    /// <summary>
    /// Gets the successful value.
    /// IMPORTANT: Accessing this property on a failed result will throw an InvalidOperationException.
    /// Always check the IsSuccess flag before accessing the Value.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when trying to access the value of a failed result.</exception>
    [NotNull]
    public TValue Value => IsSuccess
        ? _value! // The '!' is the null-forgiving operator. We promise the compiler _value is not null in a success case.
        : throw new InvalidOperationException("The value of a failure result can't be accessed.");

    /// <summary>
    /// Enables implicit conversion from a value of type TValue to a successful Result<TValue>.
    /// This allows for cleaner code, e.g., 'return myCustomer;' instead of 'return Result.Success(myCustomer);'.
    /// It gracefully handles nulls by converting them to a Failure result with a standard NullValue error.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    public static implicit operator Result<TValue>(TValue? value) =>
        value is not null ? Success(value) : Failure<TValue>(Error.NullValue);
}