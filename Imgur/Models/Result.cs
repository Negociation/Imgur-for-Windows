using Imgur.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Imgur.Models
{
    public class Result<T>
    {
        public bool IsSuccess { get; set; }
        public T Data { get; set; }
        public string Error { get; set; }
        public ErrorType ErrorType { get; set; }

        public static Result<T> Success(T data) => new Result<T>
        {
            IsSuccess = true,
            Data = data,
            ErrorType = ErrorType.None
        };

        public static Result<T> Failure(string error, ErrorType errorType = ErrorType.Unknown)
            => new Result<T>
            {
                IsSuccess = false,
                Error = error,
                ErrorType = errorType
            };
    }
}
