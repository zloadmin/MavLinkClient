using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;
using System.Reflection;
using System.Text.Json;

namespace MavLinkNet
{
    internal class Program
    {
        const string REMOTE_HOST = "172.16.8.128";
        const int REMOTE_PORT = 5555;
        const int LOCAL_PORT = 4560;

        static public int packetId = 0;
        static private MavLinkTcpServerTransport _tcpServer;
        static private UasCommandLong firstCommandLong = null;
        static private UasHeartbeat firstHeartbeat = null;
        static private UasHilSensor lastSensor = null;
        const string DUMP_10 = "10-107messages.txt";

        static void Main()
        {
            

             List<UasHilSensor> sensors = new List<UasHilSensor>();

            foreach (string line in File.ReadLines(DUMP_10)) // 10 сообщений gazebo
            {
                sensors.Add(JsonSerializer.Deserialize<UasHilSensor>(line));
            }

            UInt64 TimeUsec = 0;
            _tcpServer = new MavLinkTcpServerTransport();
            _tcpServer.OnPacketReceived += OnPacketReceived;
            _tcpServer.Initialize();
            
            while (firstCommandLong == null && firstHeartbeat == null)
            {
            }
            
            
            UasHilSensor lastHilSensor = null;
            int limit = 9;
            int i = 0;
            while (true)
            {
                Console.WriteLine(i);
                UasHilSensor sensor = sensors[i];
                sensor.FieldsUpdated = ComputeUpdatedMask(sensor);
                sensor.TimeUsec = TimeUsec;
                _tcpServer.SendMessage(sensor);
                lastHilSensor = sensor;
                TimeUsec += 4000;
                i++;
                if(i >= limit) {
                    i = 0;
                }
                Thread.Sleep(5);
            }

              
        }


        const float MAX_MAG   = 2f;
        const float MAX_GYRO  = 50f;
        const float MAX_ACCEL = 200f;
        const float MAX_TEMP  = 150f; 
        const float MAX_PRESS = 2000f;
        
        public static UInt32 ComputeUpdatedMask(UasHilSensor now)
        {
            UInt32 m = 0;
            bool accValid =
                IsValidFloat(now.Xacc, MAX_ACCEL) &&
                IsValidFloat(now.Yacc, MAX_ACCEL) &&
                IsValidFloat(now.Zacc, MAX_ACCEL);
            if (accValid)
            {
                m |= 1u << 0;
                m |= 1u << 1;
                m |= 1u << 2;    
            }
            
            bool gyroValid =
                IsValidFloat(now.Xgyro, MAX_GYRO) &&
                IsValidFloat(now.Ygyro, MAX_GYRO) &&
                IsValidFloat(now.Zgyro, MAX_GYRO);
            
            if (gyroValid)
            {
                m |= 1u << 3;
                m |= 1u << 4;
                m |= 1u << 5;    
            }   
            
            bool magValid =
                IsValidFloat(now.Xmag, MAX_MAG) &&
                IsValidFloat(now.Ymag, MAX_MAG) &&
                IsValidFloat(now.Zmag, MAX_MAG);
            
            if (magValid)
            {
                m |= 1u << 6;
                m |= 1u << 7;
                m |= 1u << 8;    
            }

            bool validAbsPressure = IsValidFloat(now.AbsPressure, MAX_PRESS);
            if (validAbsPressure)
            {
                m |= 1u << 9;
            }

            bool validDiffPressure = IsValidFloat(now.DiffPressure, MAX_PRESS); 
            if (validDiffPressure)
            {
                m |= 1u << 10;    
            }

            bool validPressureAlt = IsValidFloat(now.PressureAlt, MAX_PRESS) && now.PressureAlt > 0.1f; 
            if (validPressureAlt)
            {
                m |= 1u << 11;    
            }

            bool tempValid = IsValidFloat(now.Temperature, MAX_TEMP);
            if (tempValid)
            {
                m |= 1u << 12;
            }
            // Console.WriteLine("--------------------");
            // Console.WriteLine("accValid " + accValid);
            // Console.WriteLine("gyroValid " + gyroValid);
            // Console.WriteLine("magValid " + magValid);
            // Console.WriteLine("validAbsPressure " + validAbsPressure);
            // Console.WriteLine("validDiffPressure " + validDiffPressure);
            // Console.WriteLine("validPressureAlt " + validPressureAlt);
            // Console.WriteLine("tempValid " + tempValid);
            return m;
        }

        
        static bool IsValidFloat(float v, float maxAbs)
        {
            if (float.IsNaN(v) || float.IsInfinity(v))
                return false;

            float av = Math.Abs(v);

            // reject denormals / uninitialized memory
            if (av < 1e-6f)
                return false;
            
            if (av > maxAbs)
                return false;

            return true;
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
                Xacc = 0.036102064f,
                Yacc = -0.010861181f,
                Zacc = -0.030836519f,
                Xgyro = -0.010941599f,
                Ygyro = 0.002232928f,
                Zgyro = 0.0039140754f,
                Xmag = 0f,
                Ymag = -5.189695E+11f,
                Zmag = 10.959854f,
                AbsPressure = 0f,
                DiffPressure = 9.1834E-41f,
                PressureAlt = -4.4216272E-14f,
                Temperature = 9.1834E-41f,
                FieldsUpdated = 63
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
            if (packet.MessageId == 76 && firstCommandLong == null)
            {
                firstCommandLong = (UasCommandLong)packet.Message;
            }

            if (packet.MessageId == 0 && firstHeartbeat == null)
            {
                firstHeartbeat = (UasHeartbeat)packet.Message;
            }

            UasMessagePrinter.Print(packet.Message);

            // можно сохранять дамп hex в json
            // if (packet.MessageId == 107)
            // {
            //     File.AppendAllText(DUMP_107,
            //         JsonSerializer.Serialize((UasHilSensor)packet.Message) + Environment.NewLine);
            // }
            //
            // if (packet.MessageId == 115)
            // {
            //     File.AppendAllText(DUMP_115,
            //         JsonSerializer.Serialize((UasHilStateQuaternion)packet.Message) + Environment.NewLine);
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

        

        
    }
}