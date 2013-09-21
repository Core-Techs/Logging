using System;

namespace CoreTechs.Logging
{
    public static class Try
    {
        public static Result Do(Action action)
        {
            try
            {
                action();
                return new Result();
            }
            catch (Exception ex)
            {
                return new Result(ex);
            }
        }

        public static Result<T> Get<T>(Func<T> factory, T @default = default(T))
        {
            try
            {
                return new Result<T>(factory());
            }
            catch (Exception ex)
            {
                return new Result<T>(@default, ex);
            }
        }

        public class Result
        {
            public bool Success { get { return Exception == null; } }
            public Exception Exception { get; private set; }

            public Result(Exception exception = null)
            {
                Exception = exception;
            }
        }

        public class Result<T> : Result
        {
            public T Value { get; private set; }

            public Result(T value, Exception exception = null)
                : base(exception)
            {
                Value = value;
            }
        }
    }
}