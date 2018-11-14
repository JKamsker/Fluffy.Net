using Fluffy.IO.Recycling;

using System;

namespace Fluffy.IO.Buffer
{
    public class FakeRecycler : IRecycler<byte[]>
    {
        private static FakeRecycler _instance;
        public static FakeRecycler Instance = _instance ?? (_instance = new FakeRecycler());

        public void Recycle(byte[] @object)
        {
            Console.WriteLine("Lol. Recycled fake buffer");
        }
    }
}