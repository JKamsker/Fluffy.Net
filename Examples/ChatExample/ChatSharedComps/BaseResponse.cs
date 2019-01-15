using System;

namespace ChatSharedComps
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
    }
}