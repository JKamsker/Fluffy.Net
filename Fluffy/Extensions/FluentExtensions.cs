using System;
using System.Collections.Generic;

namespace Fluffy.Extensions
{
    public static class FluentExtensions
    {
        public static T Do<T>(this T value, Action<T> action)
        {
            action(value);
            return value;
        }

        public static void ForEach<T>(this IEnumerable<T> collection, Action<T> action)
        {
            foreach (var value in collection)
            {
                action(value);
            }
        }
    }
}