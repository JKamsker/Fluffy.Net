﻿using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.CompilerServices;
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

        private SendTransferObject HandleRequest(SendTransferObject obj)
        {
            obj.State = STOState.Response;
            try
            {
                obj.Value = _connection.PacketHandler.Handle(obj.Value);
            }
            catch (Exception e)
            {
                obj.HasFailed = true;
                obj.Value = null;
                obj.ExceptionMessage = e.ToString();
                if (e.GetType().IsSerializable)
                {
                    obj.Exception = e;
                }
            }
            return obj;
        }

        private void HandleResponse(SendTransferObject obj)
        {
            if (_wrappers.TryRemove(obj.Guid, out var wrapper))
            {
                wrapper.SetResult(obj);
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
            public abstract void SetResult(SendTransferObject value);
        }

        private class DuplexResult<T> : DuplexResult, IPacketResult<T>
        {
            private TaskCompletionSource<T> _completionSource;
            private ManualResetEvent _resetEvent;
            private volatile bool _hasCompleted = false;
            private T _value;

            public T Result => GetValue();
            public Task<T> Task => _completionSource.Task;

#if !NET40

            public TaskAwaiter<T> GetAwaiter()
            {
                return _completionSource.Task.GetAwaiter();
            }

#endif
            private static TaskCreationOptions CreationOptions
            {
                get
                {
#if NET46 || NET47 || NET472
                    //https://github.com/davidfowl/AspNetCoreDiagnosticScenarios/blob/master/AsyncGuidance.md#always-create-taskcompletionsourcet-with-taskcreationoptionsruncontinuationsasynchronously
                    return TaskCreationOptions.RunContinuationsAsynchronously;
#else
                    return TaskCreationOptions.None;
#endif
                }
            }
            public DuplexResult()
            {
                //TODO: CreationOptions
                _completionSource = new TaskCompletionSource<T>();
                _resetEvent = new ManualResetEvent(false);
            }

            private SendTransferObject _transferObject;

            public override void SetResult(SendTransferObject value)
            {
                _transferObject = value;
                T result = (T)value.Value;
                _value = result;
                _hasCompleted = true;
                if (value.HasFailed)
                {
                    _completionSource.SetException(value.Exception);
                }
                else
                {
                    _completionSource.SetResult(result);
                }

                _resetEvent.Set();
                _resetEvent.Dispose();
                _resetEvent = null;
            }

            private T GetValue()
            {
                if (!_hasCompleted)
                {
                    _resetEvent?.WaitOne();
                    throw _transferObject.Exception ?? new Exception(_transferObject.ExceptionMessage);
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

            public bool HasFailed { get; set; }
            public string ExceptionMessage { get; set; }
            public Exception Exception { get; set; }

            public SendTransferObject(object value)
            {
                Guid = Guid.NewGuid();
                State = STOState.Request;
                Value = value;
            }
        }
    }
}