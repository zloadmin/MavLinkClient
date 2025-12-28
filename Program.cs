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
        static private UasCommandLong firstCommandLong = null;
        static private UasHeartbeat firstHeartbeat = null;

        static void Main()
        {
            _tcpServer = new MavLinkTcpServerTransport();
            _tcpServer.OnPacketReceived += OnPacketReceived;
            _tcpServer.Initialize();
            
            
            

            // while (firstCommandLong == null && firstHeartbeat == null)
            // {
            //     Console.Write("W");
            // }
            
            
            UasHilSensor hilSensor = GetUasHilSensorMessage();
            UasHilStateQuaternion hilStateQuaternion = GetUasHilStateQuaternionMessage();
            List<string> list = new List<string>();
            list.Add("FD3D00001C01C86B000060E4010000000000C2DF133D18F331BCDE9CFCBC644433BC5156123BA541803B00000000FCA9F1D2905B2F4100000000FFFF0000E42147A9FFFF00003FB0EFFD4000001D01C873000060E40100000000002905353FF3E32A1D50EE2A9D2905353F7559B39ABE8E7B21C6D41E984C52401C44F417052C73070000000000710000007100000000004E2671D1"); 
            list.Add("FD3E00001D01C86B000000F4010000000000DD2B0B3D8460F1BB5E9946B95AD069BA27F360BB0001A13A6A727C3CF9EF5DBED012D83E83FC6E44FFFF00009F07F44340413D41FF1BBAD9FD4000001E01C873000000F40100000000002905353FAE81941C7F9C949C2905353FC97FBD9AA34C2F22640820984C52401C44F417052773070000000000750000007500000000004E26BF3F"); 
            list.Add("FD3D00001E01C86B0000A00302000000000098E709BD0122C7BCCF63193CE9C84D3BECFDDD3915F2EDBA00000000FCA9F1D2805B2F41FFFF0000FFFF0000F08046A9FFFF00003FD064FD4000001F01C8730000A0030200000000002905353FF741339D7B31331D2905353F25C0CE9A9FCB9E2276BE20984C52401C44F417052373070000000000790000007900000000004E2621E4"); 
            list.Add("FD3D00001F01C86B0000401302000000000063B8153C0BEB203D38D63EBCF1529E3B2BF0833B3539E5BB00000000FCA9F1D2805B2F41FFFF0000FFFF0000F08046A9FFFF00003F6338FD4000002001C873000040130200000000002905353F2F12209E1F0D201E2905353FAE51069B1845BD22EE9567984C52401C44F417051E730700000000007D0000007D00000000004E26D37E"); 
            list.Add("FD3E00002001C86B0000E0220200000000004B996EBD1FC790BCE8A07E3C078B843BDCC097BAA80DAA3AC4B41D3C35CF61BED4D7DC3EFFFF0000FFFF0000F08046A9FFFF0000FF013A29FD4000002101C8730000E0220200000000002905353FD060949EC55C941E2905353F788E5E9B0ECB63237DBD8C984C52401C44F417051973070000000000810000008100000000004E267D29"); 
            list.Add("FD3D00002101C86B0000803202000000000034A57BBCA9D3DDBC890A4BBDB57F513B865BBCBBA61907BA00000000FCA9F1D2805B2F41FFFF0000FFFF0000F08046A9FFFF00003F42F3FD4000002201C873000080320200000000002905353F57521D9F994F1D1F2905353FA0FA1D9BF2A1F522549C9E984C52401C44F417051373070000000000850000008500000000004E26DC06"); 
            list.Add("FD3D00002201C86B0000204202000000000029F696BC598089BCDD7E0F3DB6906BBAA16B1E3B9087AD3800000000FCA9F1D2805B2F41FFFF0000FFFF0000F08046A9FFFF00003F1EDBFD4000002301C873000020420200000000002905353F9B93489F5A90481F2905353FC5B1D09A6C6BA7A2DA05AE984C52401C44F417050E73070000000000890000008900000000004E261B6A"); 
            list.Add("FD3E00002301C86B0000C0510200000000008081B6BC48894BBD2D94493C2FB71938313298BB369D9FBAD8B6173C35265EBE4FD7D93E52FD6E44FFFF000091F9F34320423D41FF1B9D62FD4000002401C8730000C0510200000000002905353F6498299FC794291F2905353F6503C59A2A6D2CA33E469D984C52401C44F4170509730700000000008D0000008D00000000004E26637F"); 
            list.Add("FD3D00002401C86B000060610200000000009304813CC99837BD2C1C9ABB8720803AD35B453B098F2EBB00000000FCA9F1D2805B2F41FFFF0000FFFF0000F08046A9FFFF00003F2523FD4000002501C873000060610200000000002905353F510CD79E4405D71E2905353FF76E309BA72D0CA2DFDD9D984C52401C44F417050373070000000000910000009100000000004E26CA8B"); 
            list.Add("FD3D00002501C86B00000071020000000000091188BCF8823EBD8894273C4A00F73A2ADB0D3A09D072BB00000000FCA9F1D2805B2F41FFFF0000FFFF0000F08046A9FFFF00003F83F9FD4000002601C873000000710200000000002905353F9BDEBF9EC1D6BF1E2905353F41CE169B78D81FA1E9409F984C52401C44F41705FD72070000000000950000009500000000004E26B71A"); 
            list.Add("FD3E00002601C86B0000A0800200000000000413C5BCCD3E21BC79B615BD35788EBA8F56B73B41A491BAC756B93B82A35DBE1CADDB3EFFFF0000FFFF0000F08046A9FFFF0000FF01B6A7FD4000002701C8730000A0800200000000002905353F02B9B79E73B0B71E2905353F66F8D79AD3C7A1A2FA169D984C52401C44F41705F772070000000000980000009800000000004E26B35B"); 
            list.Add("FD3D00002701C86B00004090020000000000689B73BCC27298BBED48393D503F92BAD9319DBBEB44523B00000000FCA9F1D2805B2F41FFFF0000FFFF0000F08046A9FFFF00003FC873FD4000002801C873000040900200000000002905353F5EA5789E3F93781E2905353FF48AA99A77F91FA33CBF9A984C52401C44F41705F1720700000000009C0000009C00000000004E2694B5"); 
            list.Add("FD3D00002801C86B0000E09F020000000000F8931D3DB826AABBCA24103D2A0192BB898712BBF9C245BB00000000FCA9F1D2805B2F41FFFF0000FFFF0000F08046A9FFFF00003FA1AFFD4000002901C8730000E09F0200000000002905353FB047849C7CB0831C2905353F85CAE99A215B00A353509B984C52401C44F41705EB72070000000000A0000000A000000000004E26D309"); 
            list.Add("FD3E00002901C86B000080AF02000000000097A6BABC77FBA83D3F693FBDF0CC213BB772443A915CBC3A24D7623CBD5D58BECEA4DC3E15FD6E44FFFF0000BBFDF34320433D41FF1B7384FD4000002A01C873000080AF0200000000002905353FE44E271EF662279E2905353F8027F09A5F24C7A266759C984C52401C44F41705DE72070000000000A4000000A400000000004E265785"); 
            list.Add("FD3D00002A01C86B000020BF020000000000FB91F3BC408B823C6BE931BBBF6E98BBD6CB86BB449B853900000000FCA9F1D2805B2F41FFFF0000FFFF0000F08046A9FFFF00003F800CFD4000002B01C873000020BF0200000000002905353F88B89B1E2FC39B9E2905353F1B79019BC78281A2D86C9E984C52401C44F41705DE72070000000000A8000000A800000000004E260547"); 
            list.Add("FD3D00002B01C86B0000C0CE0200000000004B2A613C5AF8BC3CD5AF853D396E713AB5F08BBAD50B28BB00000000FCA9F1D2805B2F41FFFF0000FFFF0000F08046A9FFFF00003FED06FD4000002C01C8730000C0CE0200000000002905353FFE64CA1E5D70CA9E2905353F3C2A161A6242BFA38E0997984C52401C44F41705D772070000000000AC000000AC00000000004E26326F"); 
            list.Add("FD3E00002C01C86B000060DE020000000000C540243DC356BB3C49326B3DC8DE913BB0DF733A5B096C3B0111223CC74164BE0261DB3EFFFF0000FFFF0000F08046A9FFFF0000FF01DA11FD4000002D01C873000060DE0200000000002905353FC296711F4D9C719F2905353F77B2571A0FC10EA4F99E91984C52401C44F41705D072070000000000B0000000B000000000004E261436"); 
            list.Add("FD3D00002D01C86B000000EE020000000000960CBFBCAE21A4BB6A6D2B3AAFE02039E8043C3B089804BA00000000FCA9F1D2805B2F41FFFF0000FFFF0000F08046A9FFFF00003F3F1EFD4000002E01C873000000EE0200000000002905353F12F7DF1FC1F9DF9F2905353F2BC59B18360D0CA4B80B94984C52401C44F41705C972070000000000B4000000B400000000004E26837C"); 
            list.Add("FD3D00002E01C86B0000A0FD020000000000F0D2F2BC541E34BD9AAFE93CDE4570BAFBD85DBB6213C6BA00000000FCA9F1D2805B2F41FFFF0000FFFF0000F08046A9FFFF00003F40E2FD4000002F01C8730000A0FD0200000000002905353FAB7B2220027D22A02905353FD8CE2C9BEDA03AA38D619E984C52401C44F41705C272070000000000B8000000B800000000004E26E2FB"); 
            list.Add("FD3E00002F01C86B0000400D030000000000996E0BBA4A0219BCDB05133DDE9D7C3AA673383B858E40BB24D4593CEC2B5DBE2893DB3EDBFD6E44FFFF0000C7F0F34340443D41FF1B3C7DFD4000003001C8730000400D0300000000002905353F96DB32200CDD32A02905353FAFED149B685E1BA280C2A1984C52401C44F41705BB72070000000000BC000000BC00000000004E26B8E3"); 
            list.Add("FD3D00003001C86B0000E01C03000000000086C9F43C9B871ABD7FD91A3DECA0B33A66941DBB959936B900000000FCA9F1D2805B2F41FFFF0000FFFF0000F08046A9FFFF00003F2AFDFD4000003101C8730000E01C0300000000002905353F708E3620009036A02905353FF6DFAB9AB4C4AFA275239D984C52401C44F41705B372070000000000C0000000C000000000004E26464C"); 
            list.Add("FD3D00003101C86B0000802C03000000000024D327BB04A7C03C66301FBBA0745FBBAF46833B343B863B00000000FCA9F1D2805B2F41FFFF0000FFFF0000F08046A9FFFF00003FA8E0FD4000003201C8730000802C0300000000002905353FD7B23E2075B43EA02905353F44B9B09A557030A3B56CBD984C52401C44F41705AB72070000000000C4000000C400000000004E26F694"); 
            list.Add("FD3E00003201C86B0000203C03000000000070F7253C664DA13C3B104F3C750CC939C2EA063B195973BBF57545BA9A515EBE3608DB3EFFFF0000FFFF0000F08046A9FFFF0000FF017D54FD4000003301C8730000203C0300000000002905353FCDBC4E2096BE4EA02905353FBFB5EF9AF7AB0DA3F600B1984C52401C44F41705A372070000000000C8000000C800000000004E26A9B9"); 
            list.Add("FD3D00003301C86B0000C04B0300000000002787AB3BDD5706BD095DD93CD5AE74BB947051BA787489BA00000000FCA9F1D2805B2F41FFFF0000FFFF0000F08046A9FFFF00003F266CFD4000003401C8730000C04B0300000000002905353F556E5B2021705BA02905353F68AD2C9BCA33CD1F319AB2984C52401C44F417059372070000000000CB000000CB00000000004E264169"); 
            list.Add("FD3D00003401C86B0000605B030000000000F84A74BD744B89BC8C7ACCBBF4C3193B47B230BB39B4163B00000000FCA9F1D2805B2F41FFFF0000FFFF0000F08046A9FFFF00003FB791FD4000003501C8730000605B0300000000002905353FA4235B208D255BA02905353F718D819AEEDB0BA32223AF984C52401C44F417059372070000000000CF000000CF00000000004E261E7E");
            // string hexString = "FD3D00001C01C86B000060E4010000000000C2DF133D18F331BCDE9CFCBC644433BC5156123BA541803B00000000FCA9F1D2905B2F4100000000FFFF0000E42147A9FFFF00003FB0EFFD4000001D01C873000060E40100000000002905353FF3E32A1D50EE2A9D2905353F7559B39ABE8E7B21C6D41E984C52401C44F417052C73070000000000710000007100000000004E2671D1";
            

            while (true)
            {
                foreach (string hexString in list)
                {
                    byte[] byteArray = Convert.FromHexString(hexString);
                    _tcpServer.SendMavlinkHex(byteArray);
                    Thread.Sleep(100);
                }
                // _tcpServer.SendMessage(hilSensor);
                // _tcpServer.SendMessage(hilStateQuaternion);
                
            }

            //  while (true)
            // {
            //     // стандартный интервал (например 1000 мс)
            //     _tcpServer.SendMessage(hilSensor);
            //     Thread.Sleep(1000);

            //     // каждые 5 секунд
            //     if ((DateTime.UtcNow - last5s).TotalMilliseconds >= 5000)
            //     {
            //         _tcpServer.SendMessage(hilStateQuaternion);
            //         last5s = DateTime.UtcNow;
            //     }
            // }
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
            if (packet.MessageId == 76 && firstCommandLong == null)
            {
                firstCommandLong = (UasCommandLong)packet.Message;
            }
            if (packet.MessageId == 0 && firstHeartbeat == null)
            {
                firstHeartbeat = (UasHeartbeat)packet.Message;
            }
            
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