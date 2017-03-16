using System;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Turbina
{
    public class ClientSession
    {
        private readonly WebSocket _webSocket;
        private readonly HostProcess _host;

        public ClientSession(WebSocket webSocket, HostProcess host)
        {
            _webSocket = webSocket;
            _host = host;
        }

        public async Task Process()
        {
            var token = _host.Token;
            var buffer = new ArraySegment<byte>(new byte[4096]);

            while (_webSocket.State == WebSocketState.Open)
            {
                try
                {
                    var command = await ReadString(_webSocket, buffer, token);

                    token.ThrowIfCancellationRequested();

                    _host.SendCommandToHost(command);
                }
                catch (Exception e)
                {
                    break;
                }
            }
        }

        private static async Task<string> ReadString(WebSocket webSocket, ArraySegment<byte> buffer, CancellationToken token)
        {
            using (var ms = new MemoryStream())
            {
                WebSocketReceiveResult result;
                do
                {
                    result = await webSocket.ReceiveAsync(buffer, token);
                    ms.Write(buffer.Array, buffer.Offset, result.Count);
                } while (!result.EndOfMessage);

                ms.Seek(0, SeekOrigin.Begin);
                var streamReader = new StreamReader(ms, Encoding.UTF8);
                return streamReader.ReadToEnd();
            }
        }
    }
}