using System;

namespace Fluffy.Extensions
{
    public static class FluentExtensions
    {
        public static T Do<T>(this T value, Action<T> action)
        {
            action(value);
            return value;
        }
    }
}