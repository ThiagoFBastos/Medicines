using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Medicines.Utils
{
    public class Result<TValue, TError>
    {
        public TValue? Value { get; init; }

        public TError? Error { get; init; }

        public bool IsSuccess { get; init; }

        public static Result<TValue, TError> Success(TValue value)
        {
            return new Result<TValue, TError>
            {
                Value = value,
                IsSuccess = true
            };
        }

        public static Result<TValue, TError> Failure(TError error)
        {
            return new Result<TValue, TError>
            {
                Error = error,
                IsSuccess = false
            };
        }
    }
}
