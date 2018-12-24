//Integrated in >= NET471
#if (NET47 || NET46 || NET45 || NET40)

using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace System.Net.Sockets
{
    public static class CompatibilityExtensions
    {
        /// <summary>Establishes a connection to a remote host.</summary>
        /// <param name="socket">The socket that is used for establishing a connection.</param>
        /// <param name="remoteEP">An EndPoint that represents the remote device.</param>
        /// <returns>An asynchronous Task.</returns>
        public static Task ConnectAsync(this Socket socket, EndPoint remoteEP)
        {
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
        }
    }
}

#endif