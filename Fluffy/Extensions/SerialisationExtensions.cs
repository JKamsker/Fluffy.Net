using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Fluffy.Extensions
{
    public static class SerialisationExtensions
    {
        public static T Deserialize<T>(this byte[] serializedBytes)
        {
            return new MemoryStream(serializedBytes).Deserialize<T>();
        }

        public static T Deserialize<T>(this Stream stream)
        {
            var result = stream.Deserialize();

            if (result is T typedResult)
            {
                return typedResult;
            }
            return default;
        }

        public static object Deserialize(this Stream stream)
        {
            var bf = new BinaryFormatter();
            return bf.Deserialize(stream);
        }

        public static byte[] Serialize<T>(this T graph)
        {
            using (var ms = new MemoryStream())
            {
                graph.Serialize(ms);
                return ms.ToArray();
            }
        }

        public static void Serialize<T>(this T graph, Stream serializationStream)
        {
            var bf = new BinaryFormatter();
            bf.Serialize(serializationStream, graph);
        }
    }
}