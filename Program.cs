using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Diagnostics;

namespace MavLinkNet
{
    internal class Program
    {
        const string REMOTE_HOST = "172.16.8.128";
        const int REMOTE_PORT = 5555;
        const int LOCAL_PORT = 4560;
        

        static void Main()
        {
            ProxyServer();

        }

        static async Task ProxyServer()
        {
            var listener = new TcpListener(IPAddress.Any, LOCAL_PORT);
            listener.Start();

            Console.WriteLine($"[PROXY] Listening on TCP {LOCAL_PORT}");
            Console.WriteLine($"[PROXY] Forwarding to {REMOTE_HOST}:{REMOTE_PORT}");

            while (true)
            {
                var px4 = await listener.AcceptTcpClientAsync();
                Console.WriteLine("[PROXY] PX4 connected by port: " + LOCAL_PORT);

                TcpClient gazebo = await WaitForRemoteServerAsync();
                Console.WriteLine("[PROXY] Gazebo connected by port: " + REMOTE_PORT);

                _ = ProxyPipe(gazebo, px4, "gazebo → px4");
                _ = ProxyPipe(px4, gazebo, "px4 → gazebo");
            }
        }


        static BinaryReader CreateReader(byte[] buffer, int offset, int length)
        {
            var ms = new MemoryStream(buffer, offset, length);
            return new BinaryReader(ms);
        }
        
        static void PrintMessage(byte[] data, int length, string tag)
        {
            BinaryReader reader = CreateReader(data, 0, length);
            MavLinkPacketV20 packet = MavLinkPacketV20.Deserialize(reader, 0);
            Console.WriteLine("Message ID " + packet.MessageId);
        }
        
        

        static void PrintHex(byte[] data, int length, string tag)
        {
            Console.Write($"[{tag}] {length} bytes: ");
            for (int i = 0; i < length; i++)
                Console.Write($"{data[i]:X2} ");
            Console.WriteLine();
        }

        /// <summary>
        /// Ждём, пока на REMOTE_HOST:REMOTE_PORT появится TCP сервер
        /// </summary>
        static async Task<TcpClient> WaitForRemoteServerAsync()
        {
            while (true)
            {
                try
                {
                    var server = new TcpClient();
                    await server.ConnectAsync(REMOTE_HOST, REMOTE_PORT);
                    return server;
                }
                catch (SocketException)
                {
                    Console.WriteLine($"[PROXY] Waiting for PX4 TCP {REMOTE_HOST}:{REMOTE_PORT} ...");
                    await Task.Delay(500); // 0.5 секунды
                }
            }
        }

        static async Task ProxyPipe(TcpClient from, TcpClient to, string tag)
        {
            var fromStream = from.GetStream();
            var toStream = to.GetStream();

            byte[] buffer = new byte[4096];

            try
            {
                while (true)
                {
                    int read = await fromStream.ReadAsync(buffer, 0, buffer.Length);
                    if (read <= 0) break;
                    
                    PrintHex(buffer, read, tag);
                    PrintMessage(buffer, read, tag);
                    await toStream.WriteAsync(buffer, 0, read);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{tag}] ERROR: {ex.Message}");
            }
            finally
            {
                from.Close();
                to.Close();
                Console.WriteLine($"[{tag}] Connection closed");
            }
        }

        
    }
}