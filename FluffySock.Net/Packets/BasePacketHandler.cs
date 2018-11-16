using Fluffy.Extensions;
using Fluffy.IO.Buffer;

using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Fluffy.Net.Packets
{
    public enum Packet : byte
    {
        TestPacket = 1,
        FormattedPacket
    }

    public abstract class BasePacket
    {
        public abstract byte OpCode { get; }

        internal ConnectionInfo Connection { get; set; }

        public abstract void Handle(LinkedStream stream);
    }

    public class DummyPacket : BasePacket
    {
        public override byte OpCode => (int)Packet.TestPacket;

        public override void Handle(LinkedStream stream)
        {
            using (var sr = new StreamReader(stream))
            {
                Console.WriteLine(sr.ReadToEnd());
            }
        }
    }

    public class FormattedPacket : BasePacket
    {
        public override byte OpCode => (int)Packet.FormattedPacket;

        public override void Handle(LinkedStream stream)
        {
            var handleObject = stream.Deserialize();
            var result = Connection.TypedPacketHandler.Handle(handleObject);
            if (result != null)
            {
                if (result is Task task)
                {
                    if (TaskUtility.IsGenericTask(task))
                    {
                        if (task.IsCompleted)
                        {
                            FinishTask(task);
                        }
                        else
                        {
                            task.ContinueWith(FinishTask);
                        }
                    }
                }
                else
                {
                    SendResult(result);
                }
            }
        }

        private void FinishTask(Task task)
        {
            if (task.Status == TaskStatus.RanToCompletion)
            {
                SendResult(TaskUtility.GetResultIl(task));
            }
            else
            {
                //TODO: Excepion Handling
                Debug.WriteLine($"Exception while handling packet:{task.Exception}");
            }
        }

        private void SendResult<T>(T value)
        {
            Connection.Sender.Send(value);
        }
    }
}