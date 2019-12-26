using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fluffy.Net.Compatibility
{
#if NET40
    public class ReadOnlyCollectionWrapper<T> : ReadOnlyCollection<T>, IReadOnlyCollection<T>
    {
        public ReadOnlyCollectionWrapper(IList<T> list) : base(list)
        {
        }
    }



    /// <summary>Represents a strongly-typed, read-only collection of elements.</summary>
    /// <typeparam name="T">The type of the elements.</typeparam>
    public interface IReadOnlyCollection<out T> : IEnumerable<T>, IEnumerable
    {
        /// <summary>Gets the number of elements in the collection.</summary>
        /// <returns>The number of elements in the collection. </returns>
        int Count { get; }
    }
#endif
    public static class ReadOnlyCollectionExtensions
    {
        public static IReadOnlyCollection<T> ToReadOnly<T>(this IList<T> list)
        {
#if NET40
            return new ReadOnlyCollectionWrapper<T>(list);
#else
            return new ReadOnlyCollection<T>(list);
#endif
        }
    }

}
