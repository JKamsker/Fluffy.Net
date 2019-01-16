using System;

namespace Fluffy.Net.Packets.Modules.Formatted
{
    [Serializable]
    public class BaseResponse
    {
        public string Message { get; set; }
        public bool Success { get; set; }

        public BaseResponse(bool success, string message = "")
        {
            Success = success;
            Message = message;
        }

        public BaseResponse()
        {
        }

        public static implicit operator bool(BaseResponse d)
        {
            return d?.Success ?? false;
        }
    }
}