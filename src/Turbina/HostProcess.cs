using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Turbina
{
    public class HostProcess
    {
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private readonly Process _process;
        private readonly List<WebSocket> _webSockets = new List<WebSocket>();
        private TcpClient _hostClient;
        private StreamReader _reader;
        private StreamWriter _writer;

        public HostProcess()
        {
            _process = StartProcess();
            Thread.Sleep(1000);
            _hostClient = new TcpClient();
            _hostClient.ConnectAsync(IPAddress.Loopback, 57000).GetAwaiter().GetResult();
            var networkStream = _hostClient.GetStream();
            _reader = new StreamReader(networkStream);
            _writer = new StreamWriter(networkStream);
            Task.Run(RedirectProcessOutputToWebSocket);
        }

        public void AddClient(WebSocket webSocket)
        {
            lock(_webSockets) _webSockets.Add(webSocket);
        }

        public CancellationToken Token => _cts.Token;

        public void SendCommandToHost(string command)
        {
            lock (this)
            {
                _writer.WriteLine(command);
                _writer.Flush();
            }
        }

        private void ProcessOnExited(object sender, EventArgs eventArgs)
        {
            _cts.Cancel();

            WebSocket[] webSockets;
            lock (_webSockets)
            {
                webSockets = _webSockets.ToArray();
            }

            var closeTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            foreach (var webSocket in webSockets)
            {
                webSocket.CloseOutputAsync(WebSocketCloseStatus.EndpointUnavailable, null, closeTokenSource.Token);
            }
        }

        private async Task RedirectProcessOutputToWebSocket()
        {
            while (true)
            {
                var line = await _reader.ReadLineAsync();
                if (_cts.IsCancellationRequested || line == null)
                {
                    break;
                }

                var data = Encoding.UTF8.GetBytes(line);
                WebSocket[] webSockets;
                lock (_webSockets)
                {
                    webSockets = _webSockets.ToArray();
                }

                foreach (var webSocket in webSockets)
                {
                    await webSocket.SendAsync(new ArraySegment<byte>(data), WebSocketMessageType.Text, true, _cts.Token);
                }
            }
        }

        private Process StartProcess()
        {
            var currentDirectory = Directory.GetCurrentDirectory();
            var workingDirectory = Path.Combine(currentDirectory, @"..\Turbina.Host\bin\Debug\netcoreapp1.1\");

            var process = new Process
            {
                StartInfo = new ProcessStartInfo("dotnet", "Turbina.Host.dll")
                {
                    WorkingDirectory = workingDirectory
                },
                EnableRaisingEvents = true
            };

            process.Exited += ProcessOnExited;

            process.Start();

            return process;
        }

        public void RemoveClient(WebSocket webSocket)
        {
            lock (_webSockets) _webSockets.Remove(webSocket);
        }
    }
}