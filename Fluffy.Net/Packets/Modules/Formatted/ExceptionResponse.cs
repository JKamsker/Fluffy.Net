using System;

namespace Fluffy.Net.Packets.Modules.Formatted
{
    [Serializable]
    public class ExceptionResponse : BaseResponse
    {
        public Exception Exception { get; }

        public ExceptionResponse(Exception exception)
        {
            Success = false;
            Message = exception.ToString();

            if (exception.GetType().IsSerializable)
            {
                Exception = exception;
            }
        }
    }
}