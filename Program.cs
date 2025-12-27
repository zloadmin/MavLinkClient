using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;

namespace MavLinkNet
{
    
    internal class Program
    {
        const string REMOTE_HOST = "172.16.8.128";
        const int REMOTE_PORT = 5555;
        const int LOCAL_PORT = 4560;
        static public int packetId = 0;
        
        
        

        static async Task Main()
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
        
        
        
        static async Task ProxyServer()
        {
            // var listener = new TcpListener(IPAddress.Any, LOCAL_PORT);
            // listener.Start();

            // Console.WriteLine($"[PROXY] Listening on TCP {LOCAL_PORT}");
            // Console.WriteLine($"[PROXY] Forwarding to {REMOTE_HOST}:{REMOTE_PORT}");

            // while (true)
            // {
            //     var px4 = await listener.AcceptTcpClientAsync();
            //     Console.WriteLine("[PROXY] PX4 connected by port: " + LOCAL_PORT);

            //     TcpClient gazebo = await WaitForRemoteServerAsync();
            //     Console.WriteLine("[PROXY] Gazebo connected by port: " + REMOTE_PORT);

            //     _ = ProxyPipe(gazebo, px4, "gazebo → px4");
            //     _ = ProxyPipe(px4, gazebo, "px4 → gazebo");
            // }
        }


        
        

        public static List<byte[]> Split(byte[] data)
        {
            List<byte> _buffer = new();
            var packets = new List<byte[]>();

            if (data == null || data.Length == 0)
                return packets;

            _buffer.AddRange(data);

            int i = 0;
            while (i < _buffer.Count)
            {
                byte magic = _buffer[i];

                // MAVLink v1
                if (magic == 0xFE)
                {
                    if (_buffer.Count - i < 6)
                        break;

                    int payloadLen = _buffer[i + 1];
                    int packetLen = 6 + payloadLen + 2;

                    if (_buffer.Count - i < packetLen)
                        break;

                    packets.Add(_buffer.GetRange(i, packetLen).ToArray());
                    i += packetLen;
                }
                // MAVLink v2
                else if (magic == 0xFD)
                {
                    if (_buffer.Count - i < 10)
                        break;

                    int payloadLen = _buffer[i + 1];
                    byte incompatFlags = _buffer[i + 2];
                    bool signed = (incompatFlags & 0x01) != 0;

                    int packetLen = 10 + payloadLen + 2 + (signed ? 13 : 0);

                    if (_buffer.Count - i < packetLen)
                        break;

                    packets.Add(_buffer.GetRange(i, packetLen).ToArray());
                    i += packetLen;
                }
                else
                {
                    // мусор
                    i++;
                }
            }

            // удалить обработанные байты
            if (i > 0)
                _buffer.RemoveRange(0, i);

            return packets;
        }
        
        
        

        static void PrintHex(byte[] data, int length, string tag)
        {
            // Console.WriteLine();
            // Console.WriteLine($"[{tag}] {length} bytes: ");
            // for (int i = 0; i < length; i++)
            //     Console.Write($"{data[i]:X2} ");
            // Console.WriteLine();
        }
        static void PrintMessage(byte[] data, int length, string tag)
        {
            // Console.WriteLine();
            List<byte[]> messageHexes = Split(data);
            // Console.WriteLine($"[{tag}] {length} bytes messages: " + messageHexes.Count);
            foreach (var messageHex in messageHexes)
            {
                Console.WriteLine();
                for (int i = 0; i < messageHex.Length; i++)
                    Console.Write($"{messageHex[i]:X2} ");
                Console.WriteLine();
                MavlinkMessage msg = MavlinkMessage.Deserialize(messageHex);
                Console.WriteLine($"[{tag}] Message ID: " + msg.MessageId);
            }
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