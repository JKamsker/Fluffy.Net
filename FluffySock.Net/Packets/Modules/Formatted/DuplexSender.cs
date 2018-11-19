using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Fluffy.Net.Packets.Modules.Formatted
{
    internal class DuplexSender
    {
        private readonly ConnectionInfo _connection;

        private ConcurrentDictionary<Guid, DuplexResult> _wrappers;

        public DuplexSender(ConnectionInfo connection)
        {
            _connection = connection;
            _wrappers = new ConcurrentDictionary<Guid, DuplexResult>();
            _connection.PacketHandler.On<SendTransferObject>(x => x.State == STOState.Request).Do(HandleRequest);
            _connection.PacketHandler.On<SendTransferObject>(x => x.State == STOState.Response).Do(HandleResponse);
        }

        private object HandleRequest(SendTransferObject obj)
        {
            obj.State = STOState.Response;
            obj.Value = _connection.PacketHandler.Handle(obj.Value);
            return obj;
        }

        private void HandleResponse(SendTransferObject obj)
        {
            if (_wrappers.TryRemove(obj.Guid, out var wrapper))
            {
                wrapper.SetResult(obj.Value);
            }
            else
            {
                Debug.WriteLine($"{obj.Guid} Cannot resolve STO {obj.Value}");
            }
        }

        public IPacketResult<TResult> Send<TResult>(object value)
        {
            var sto = new SendTransferObject(value);
            var result = new DuplexResult<TResult>();
            _wrappers[sto.Guid] = result;
            _connection.Sender.Send(sto);
            return result;
        }

        private abstract class DuplexResult
        {
            public abstract void SetResult(object value);
        }

        private class DuplexResult<T> : DuplexResult, IPacketResult<T>
        {
            private TaskCompletionSource<T> _completionSource;
            private ManualResetEvent _resetEvent;
            private volatile bool _hasCompleted = false;
            private T _value;

            public T Value => GetValue();
            public Task<T> Task => _completionSource.Task;

            public DuplexResult()
            {
                _completionSource = new TaskCompletionSource<T>();
                _resetEvent = new ManualResetEvent(false);
            }

            public override void SetResult(object value)
            {
                T result = (T)value;

                _completionSource.SetResult(result);
                _value = result;
                _hasCompleted = true;
                _resetEvent.Set();
            }

            private T GetValue()
            {
                if (!_hasCompleted)
                {
                    _resetEvent.WaitOne();
                }

                return _value;
            }
        }

        private enum STOState
        {
            Request,
            Response
        }

        [Serializable]
        private class SendTransferObject
        {
            public STOState State { get; set; }
            public Guid Guid { get; }
            public object Value { get; set; }

            public SendTransferObject(object value)
            {
                Guid = Guid.NewGuid();
                State = STOState.Request;
                Value = value;
            }
        }
    }
}