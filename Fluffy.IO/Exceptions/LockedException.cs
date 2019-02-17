using System;

namespace Fluffy.IO.Exceptions
{
    public class LockedException : Exception
    {
        public LockedException(string streamIsLocked) : base(streamIsLocked)
        {
        }
    }
}