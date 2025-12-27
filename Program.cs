using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;
using System.Reflection;

namespace MavLinkNet
{
    internal class Program
    {
        const string REMOTE_HOST = "172.16.8.128";
        const int REMOTE_PORT = 5555;
        const int LOCAL_PORT = 4560;
        static public int packetId = 0;
        static private MavLinkTcpServerTransport _tcpServer;


        static void Main()
        {
            _tcpServer = new MavLinkTcpServerTransport();
            _tcpServer.OnPacketReceived += OnPacketReceived;
            _tcpServer.Initialize();
            _tcpServer.HeartBeatUpdateRateMs = 100;
            // _tcpServer.BeginHeartBeatLoop();
            UasHilSensor hilSensor = GetUasHilSensorMessage();
            UasHilStateQuaternion hilStateQuaternion = GetUasHilStateQuaternionMessage();

            while (true)
            {
                Thread.Sleep(1000);
                // _tcpServer.SendMessage(hilSensor);
                _tcpServer.SendMessage(hilStateQuaternion);
            }
        }

        // Создан на основе газеповского сообщения  
        // FD3E00002901C86B0000E07B08000000000097A6BABC77FBA83D3F693FBDF0CC213BB772443A915CBC3A24D7623CBD5D58BECEA4DC3E15FD6E44FFFF0000BBFDF34320433D41FF1BD9CF
        // Должен совпадать hex, кроме TimeUsec, sequence, и Checksum
        // (все совпадает кроме обрезки 0xFF1B 2 бита, а должно быть uint32 4 бита 0xFF1B0000)
        // fd40000002ff006b0000d3b00a710000000097a6babc77fba83d3f693fbdf0cc213bb772443a915cbc3a24d7623cbd5d58becea4dc3e15fd6e44ffff0000bbfdf34320433d41ff1b0000f12e
        // и поэтому размер 0x40 (64 бита) а не 0x3E (62 бита)
        // TODO:  возможно нужно обрезать FieldsUpdated до 16 байт, так как сам газепо шлет 16, НО
        // в спецификации указано fields_updated	uint32_t		HIL_SENSOR_UPDATED_FLAGS	Bitmap for fields that have updated since last message
        static UasHilSensor GetUasHilSensorMessage()
        {
            return new UasHilSensor
            {
                SystemId = 255,
                ComponentId = 0,
                TimeUsec = unchecked((uint)Environment.TickCount),
                Xacc = -0.022784514f,
                Yacc = 0.08251088f,
                Zacc = -0.04673123f,
                Xgyro = 0.0024688803f,
                Ygyro = 0.00074939005f,
                Zgyro = 0.0014370849f,
                Xmag = 0.013845239f,
                Ymag = -0.21129508f,
                Zmag = 0.43094486f,
                AbsPressure = 955.9544f,
                DiffPressure = 9.1834E-41f,
                PressureAlt = 487.98227f,
                Temperature = 11.828888f,
                FieldsUpdated = 7167
            };
        }

        // Создан на основе газеповского сообщения  
        // FD4000002A01C8730000E07B0800000000002905353FE44E271EF662279E2905353F8027F09A5F24C7A266759C984C52401C44F41705DE72070000000000A4000000A400000000004E260F2B
        // Должен совпадать hex, кроме TimeUsec, sequence, и Checksum
        // все совпадает 
        // fd4000000001c87300006b578871000000002905353fe44e271ef662279e2905353f8027f09a5f24c7a266759c984c52401c44f41705de72070000000000a4000000a400000000004e260760
        static UasHilStateQuaternion GetUasHilStateQuaternionMessage()
        {
            return new UasHilStateQuaternion
            {
                SystemId = 1,
                ComponentId = 200,
                TimeUsec = unchecked((uint)Environment.TickCount),
                AttitudeQuaternion = [0.70711f, 8.8572206e-21f, -8.8613710e-21f, 0.70711f],
                Rollspeed = -9.9325489e-23f,
                Pitchspeed = -5.3977567e-18f,
                Yawspeed = -4.0443597e-24f,
                Lat = 473977420,
                Lon = 85455940,
                Alt = 488158,
                Vx = 0,
                Vy = 0,
                Vz = 164,
                IndAirspeed = 0,
                TrueAirspeed = 164,
                Xacc = 0,
                Yacc = 0,
                Zacc = 9806,
            };
        }

        static void OnPacketReceived(object sender, MavLinkPacketBase packet)
        {
            UasMessage message = packet.Message;
            UasMessagePrinter.Print(message);
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