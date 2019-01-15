using System;
using System.Collections.Generic;
using ChatSharedComps.Messaging;

namespace ChatSharedComps.Login
{
    [Serializable]
    public class LoginRequest
    {
        public string Name { get; set; }
        public string Password { get; set; }
    }

    [Serializable]
    public class LoginResponse : BaseResponse
    {
        public List<ChatUser> ChatUsers { get; set; }

        public LoginResponse()
        {
        }

        public LoginResponse(bool success, string message = "") : base(success, message)
        {
        }
    }
}