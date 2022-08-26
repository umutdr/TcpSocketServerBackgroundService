using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TcpSocketServerBackgroundService
{
    public class TcpSocketServerBackgroundService : BackgroundService
    {
        private static readonly Socket ServerSocket = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private const int RECEIVE_BUFFER_SIZE = 4 * 1024;
        private static readonly byte[] RECEIVE_BUFFER = new byte[RECEIVE_BUFFER_SIZE];

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await StartAsync(IPAddress.Any, 3333);
        }

        private async static Task StartAsync(IPAddress ipAddress, int port)
        {
            IPEndPoint ipEndPoint = SocketHelper.Bind(ServerSocket, ipAddress, port);
            ServerSocket.Listen();
            Log.Information("Socket server listening on {0}:{1}", ipEndPoint.Address, ipEndPoint.Port);
            await AcceptAsync(ServerSocket);
        }

        private async static Task AcceptAsync(Socket serverSocket)
        {
            Socket clientSocket = null;
            try
            {
                clientSocket = await serverSocket.AcceptAsync();
                SocketHelper.AddToClientsList(clientSocket);
                Log.Information("{0} Connected", clientSocket.RemoteEndPoint);
            }
            catch (ObjectDisposedException ex)
            {
                Log.Error(ex, string.Empty);
            }
            catch (Exception ex)
            {
                Log.Error(ex, string.Empty);
            }
            finally
            {
                // Do not await
                AcceptAsync(serverSocket);

                if (clientSocket?.IsConnected() == true)
                {
                    // Do not await
                    ReceiveAsync(clientSocket);
                }
            }
        }

        private async static Task ReceiveAsync(Socket clientSocket)
        {
            try
            {
                var receivedBytesCount = await clientSocket.ReceiveAsync(RECEIVE_BUFFER, SocketFlags.None);
                Log.Information("Client {0} send {1} bytes", clientSocket.RemoteEndPoint, receivedBytesCount);

                if (receivedBytesCount == 0)
                {
                    Log.Information("{0} Disconnected", clientSocket.RemoteEndPoint);
                    clientSocket.CloseThenRemove();
                    return;
                }

                byte[] receivedBuffer = new byte[receivedBytesCount];
                Array.Copy(RECEIVE_BUFFER, receivedBuffer, receivedBytesCount);
                string msg = Encoding.ASCII.GetString(receivedBuffer).TrimEnd(new char[] { (char)0 });

                // await SendAsync(clientSocket, string.Format("{0}", receivedBytesCount));
            }
            catch (SocketException ex)
            {
                Log.Error(ex, "SocketException error occured on SocketServer.ReceiveAsync method. SocketErrorCode = {0}", ex.SocketErrorCode);
                clientSocket.CloseThenRemove();
            }
            catch (ObjectDisposedException ex)
            {
                Log.Error(ex, "ObjectDisposedException error occured on SocketServer.ReceiveAsync method.");
                clientSocket.CloseThenRemove();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occured on SocketServer.ReceiveAsync method.");
                clientSocket.CloseThenRemove();
            }
            finally
            {
                if (clientSocket?.IsConnected() == true)
                {
                    // Do not await
                    ReceiveAsync(clientSocket);
                }
                else
                {
                    clientSocket.CloseThenRemove();
                }
            }
        }

        private async static Task SendAsync(Socket clientSocket, string mesage)
        {
            byte[] messageBytes = Encoding.ASCII.GetBytes(mesage);
            await clientSocket.SendAsync(messageBytes, SocketFlags.None);
        }

    }
}
