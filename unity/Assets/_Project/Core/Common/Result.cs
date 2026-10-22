namespace FootbalSim.Core.Common
{
    /// <summary>An expected, non-exceptional failure (locked door, no money, invalid slot).</summary>
    public readonly struct Result
    {
        public bool IsSuccess { get; }
        public string Error { get; }

        private Result(bool success, string error)
        {
            IsSuccess = success;
            Error = error;
        }

        public static Result Ok() => new Result(true, null);
        public static Result Fail(string error) => new Result(false, error);
    }

    public readonly struct Result<T>
    {
        public bool IsSuccess { get; }
        public T Value { get; }
        public string Error { get; }

        private Result(bool success, T value, string error)
        {
            IsSuccess = success;
            Value = value;
            Error = error;
        }

        public static Result<T> Ok(T value) => new Result<T>(true, value, null);
        public static Result<T> Fail(string error) => new Result<T>(false, default, error);
    }
}
