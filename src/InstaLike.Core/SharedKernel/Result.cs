using System;
using System.Linq;

namespace InstaLike.Core.SharedKernel
{
    public interface IResult
    {
        bool IsSuccess { get; }
        bool IsFailure { get; }
        Error Error { get; }
    }

    public interface IResult<out TValue> : IResult
    {
        TValue Value { get; }
    }

    /// <summary>
    /// Represents the outcome of an operation that either succeeds or fails with an <see cref="SharedKernel.Error"/>.
    /// </summary>
    public sealed class Result : IResult
    {
        private static readonly Result Succeeded = new(Error.None);

        public Error Error { get; }
        public bool IsSuccess => Error == Error.None;
        public bool IsFailure => !IsSuccess;

        private Result(Error error)
        {
            Error = error;
        }

        public static Result Ok() => Succeeded;

        public static Result<TValue> Ok<TValue>(TValue value)
            where TValue : notnull =>
            new(value, Error.None);

        public static Result Failure(Error error) => new(EnsureFailure(error));

        public static Result<TValue> Failure<TValue>(Error error)
            where TValue : notnull =>
            new(default, EnsureFailure(error));

        public static Result FailureIf(bool condition, Error error) =>
            condition ? Failure(error) : Ok();

        /// <summary>
        /// Succeeds when every result succeeds; otherwise fails with the only error, or with a <see cref="MultipleErrors"/> that groups all of them.
        /// </summary>
        public static Result Combine(params IResult[] results)
        {
            var errors = results
                .Where(r => r.IsFailure)
                .SelectMany(r => r.Error is MultipleErrors multiple ? multiple.Errors : [r.Error])
                .ToList();

            return errors.Count switch
            {
                0 => Ok(),
                1 => Failure(errors[0]),
                _ => Failure(new MultipleErrors(errors))
            };
        }

        private static Error EnsureFailure(Error error) =>
            error == Error.None
                ? throw new ArgumentException("A failure needs an error.", nameof(error))
                : error;
    }

    /// <summary>
    /// Represents the outcome of an operation that either produces a value or fails with an <see cref="SharedKernel.Error"/>.
    /// </summary>
    public sealed class Result<TValue> : IResult<TValue>
        where TValue : notnull
    {
        private readonly TValue? _value;

        public Error Error { get; }
        public bool IsSuccess => Error == Error.None;
        public bool IsFailure => !IsSuccess;

        public TValue Value =>
            IsSuccess
                ? _value!
                : throw new InvalidOperationException("There is no value after a failure.");

        internal Result(TValue? value, Error error)
        {
            _value = value;
            Error = error;
        }

        public static implicit operator Result<TValue>(TValue value) => Result.Ok(value);
    }
}
