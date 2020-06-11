using System;

namespace DataLinker.Services.Exceptions
{

    public class BaseApiException : Exception
    {
        public BaseApiException(string message) : base(message)
        {
            Message = message;
        }

        public BaseApiException() { }

        public new readonly string Message;
    }
}
