using System;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace Turbina
{
    public static class WSService
    {
        private static HostProcess HostProcess = new HostProcess();

        public static async Task HandleConnection(WebSocket webSocket)
        {
            if (HostProcess.Token.IsCancellationRequested)
            {
                HostProcess = new HostProcess();
            }

            HostProcess.AddClient(webSocket);
            var clientSession = new ClientSession(webSocket, HostProcess);

            try
            {
                await clientSession.Process();
            }
            catch (Exception)
            {
                // todo
            }

            HostProcess.RemoveClient(webSocket);
        }
    }
}