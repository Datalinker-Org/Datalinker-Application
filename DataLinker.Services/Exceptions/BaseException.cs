using System;

namespace DataLinker.Services.Exceptions
{
    public class BaseException : Exception
    {
        public BaseException(string message) : base(message)
        {
            Message = message;
        }

        public BaseException() { }

        public new readonly string Message;
    }

    public class EmailExpiredException : Exception
    {
        public EmailExpiredException(string message) : base(message)
        {
            Message = message;
        }

        public EmailExpiredException() { }

        public new readonly string Message;
    }
}