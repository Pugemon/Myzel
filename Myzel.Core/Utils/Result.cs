namespace Myzel.Core.Utils
{
    /// <summary>
    /// Represents the result of an operation, which can either be successful or failed.
    /// </summary>
    /// <typeparam name="T">The type of the value associated with the result.</typeparam>
    public class Result<T>
    {
        /// <summary>
        /// Gets the value associated with the successful result.
        /// </summary>
        public T Value { get; }

        /// <summary>
        /// Gets a value indicating whether the result is successful.
        /// </summary>
        public bool IsSuccess => ErrorMessages.Count == 0;

        /// <summary>
        /// Gets the collection of error messages associated with the failed result.
        /// </summary>
        public IReadOnlyList<string> ErrorMessages { get; }

        // Private constructor to prevent external instantiation.
        private Result(T value, List<string>? errorMessages)
        {
            Value = value;
            ErrorMessages = errorMessages ?? [];
        }

        /// <summary>
        /// Creates a successful result with the specified value.
        /// </summary>
        /// <param name="value">The value associated with the successful result.</param>
        /// <returns>A successful result.</returns>
        public static Result<T> Success(T value) => new Result<T>(value, []);

        /// <summary>
        /// Creates a failed result with the specified error message.
        /// </summary>
        /// <param name="errorMessage">The error message associated with the failed result.</param>
        /// <returns>A failed result with the specified error message.</returns>
        public static Result<T> Failure(string errorMessage) => new Result<T>(default!, new List<string> { errorMessage });

        /// <summary>
        /// Creates a failed result with the specified collection of error messages.
        /// </summary>
        /// <param name="errorMessages">The collection of error messages associated with the failed result.</param>
        /// <returns>A failed result with the specified collection of error messages.</returns>
        public static Result<T> Failure(IEnumerable<string> errorMessages) =>
            new Result<T>(default!, new List<string>(errorMessages));
    }
}
