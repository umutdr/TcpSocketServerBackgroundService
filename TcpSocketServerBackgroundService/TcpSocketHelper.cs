using Serilog;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace TcpSocketServerBackgroundService
{
    public static class SocketHelper
    {
        public static readonly List<Socket> ClientSocketsList = new();

        public static IPEndPoint Bind(Socket socketToBind, IPAddress ipAdress, int port)
        {
            IPEndPoint ipEndPoint = new IPEndPoint(ipAdress, port);
            socketToBind.Bind(ipEndPoint);

            return ipEndPoint;
        }

        public static string GetIpAddressFromSocketClientEndpoint(EndPoint clientSocketRemoteEndpoint)
        {
            var ipAddress = "undefined";
            try
            {
                ipAddress = (clientSocketRemoteEndpoint as IPEndPoint).Address.ToString();
            }
            catch (Exception ex)
            {
                Log.Error(ex, string.Empty);
            }
            return ipAddress;
        }

        public static bool CloseThenRemove(this Socket socket)
        {
            if (socket == null)
            {
                return false;
            }

            try
            {
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();

                return ClientSocketsList.Remove(socket);
            }
            catch (ObjectDisposedException ex)
            {
                Log.Error(ex, string.Empty);
            }
            catch (SocketException ex)
            {
                Log.Error(ex, string.Empty);
            }
            catch (Exception ex)
            {
                Log.Error(ex, string.Empty);
            }

            return false;
        }

        public static bool AddToClientsList(this Socket socket)
        {
            bool result = socket.IsConnected();
            if (result)
                ClientSocketsList.Add(socket);

            return result;
        }

        public static bool IsConnected(this Socket socket)
        {
            try
            {
                return !((socket.Poll(1, SelectMode.SelectRead) && socket.Available == 0) || !socket.Connected);
            }
            catch (SocketException ex)
            {
                Log.Warning(ex, string.Empty);
                return false;
            }
        }
    }
}
