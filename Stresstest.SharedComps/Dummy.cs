using System;

namespace Stresstest.SharedComps
{
    [Serializable]
    public class Dummy
    {
        public Guid Identifier { get; set; }
        public DateTime StartDateTime { get; set; }
        public int Counter { get; set; }
        public byte[] Data { get; set; }

        public Dummy()
        {
            Identifier = Guid.NewGuid();
        }
    }
}