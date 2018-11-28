using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Fluffy.Net.Utilities
{
    public static class AsyncSocketHelper
    {
        /// <summary>Establishes a connection to a remote host.</summary>
        /// <param name="socket">The socket that is used for establishing a connection.</param>
        /// <param name="remoteEP">An EndPoint that represents the remote device.</param>
        /// <returns>An asynchronous Task.</returns>
        public static Task ConnectAsync(Socket socket, EndPoint remoteEP)
        {
#if NET471
            return socket.ConnectAsync(remoteEP);
#else
            TaskCompletionSource<bool> completionSource = new TaskCompletionSource<bool>((object)socket);
            socket.BeginConnect(remoteEP, (AsyncCallback)(iar =>
            {
                TaskCompletionSource<bool> asyncState = (TaskCompletionSource<bool>)iar.AsyncState;
                try
                {
                    ((Socket)asyncState.Task.AsyncState).EndConnect(iar);
                    asyncState.TrySetResult(true);
                }
                catch (Exception ex)
                {
                    asyncState.TrySetException(ex);
                }
            }), (object)completionSource);
            return (Task)completionSource.Task;
#endif
        }
    }
}