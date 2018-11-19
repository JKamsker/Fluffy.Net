using System.Diagnostics;
using System.Threading.Tasks;
using Fluffy.Extensions;
using Fluffy.IO.Buffer;
using Fluffy.Net.Packets.Raw;

namespace Fluffy.Net.Packets.Modules.Raw
{
    public class FormattedPacket : BasePacket
    {
        public override byte OpCode => (int)PacketTypes.FormattedPacket;

        public override void Handle(LinkedStream stream)
        {
            var handleObject = stream.Deserialize();
            var result = Connection.PacketHandler.Handle(handleObject);
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