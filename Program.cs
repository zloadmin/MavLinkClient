using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace MavLinkNet
{
    class Program
    {
        static MavLinkUdpTransport udpTransport;


        static void Main()
        {
            // Console.WriteLine("test");
            udpTransport = new MavLinkUdpTransport();
            // udpTransport.OnPacketReceived += OnPacketReceived;
            udpTransport.Initialize();
            // udpTransport.BeginHeartBeatLoop();
            UasCommandLong arm = new UasCommandLong();
            
            arm.Param1 = 1;
            arm.Param2 = 0;
            arm.Param3 = 0;
            arm.Param4 = 0;
            arm.Param5 = 0;
            arm.Param6 = 0;
            arm.Param7 = 0;
            arm.Command = MavCmd.ComponentArmDisarm;
            arm.Confirmation = 1;
            // BinaryWriter s;
            // arm.DeserializeBody(s, out s);
            Console.WriteLine("ARM");
            udpTransport.SendMessage(arm);
            
            
            Thread.Sleep(200);
            
            UasSetMode offboard = new UasSetMode();
            offboard.CustomMode = 6;
            offboard.BaseMode = 1;
            offboard.TargetSystem = 1;
            Console.WriteLine("OFFBOARD");
            udpTransport.SendMessage(offboard);
            Thread.Sleep(200);

            // while (true)
            // {
            //     udpTransport.SendMessage(arm);
            //     udpTransport.SendMessage(offboard);
            //     
            // }
            // UasSetPositionTargetLocalNed navigate = new UasSetPositionTargetLocalNed();
            // navigate.TimeBootMs = DateTimeToUInt32Timestamp(DateTime.UtcNow);
            // navigate.CoordinateFrame = MavFrame.BodyNed;
            // navigate.X = 0.0f;
            // navigate.Y = 0.0f;
            // navigate.Z = -1.0f;
            // navigate.Vx = 0.0f;
            // navigate.Vy = 0.0f;
            // navigate.Vz = 0.0f;
            // navigate.Afx = 0.0f;
            // navigate.Afy = 0.0f;
            // navigate.Afz = 0.0f;
            // while (true)
            // {
            //     udpTransport.SendMessage(navigate);
            //     Thread.Sleep(1000);
            // }
            
            // Console.ReadLine();
            
            // UasParamValue takeOff = new UasParamValue();
            // takeOff.ParamId[0] = (char)0; 
            // takeOff.ParamId[1] = (char)0; 
            // takeOff.ParamId[2] = (char)0; 
            // takeOff.ParamId[7] = (char)1.0;
            // udpTransport.SendMessage(takeOff);
            // Console.ReadLine();
            
            // int Iteration = 0;
            // while (true)
            // {
            //     UasStatustext statustext = new UasStatustext();
            //     statustext.Text = getStringAsFiftyByteArray("Iteration " + Iteration);
            //     udpTransport.SendMessage(statustext);
            //     Thread.Sleep(1000);
            //     Iteration++;
            //     Console.WriteLine("Iteration " + Iteration);
            // }
            
            // UasStatustext text = new UasStatustext();
            //
            // char[] mText = new char[50];
            //
            // while (true)
            // {
            //     udpTransport.SendMessage(takeOff);
            //     Thread.Sleep(1000);
            // }
            //
            // Console.ReadLine();
            //
        }

        static char[] getStringAsFiftyByteArray(string source)
        {
            char[] buffer = new char[50]; int i = 0;
            foreach (char c in source)
            {
                buffer[i++] = c;
            }
            return buffer;
        }

        static void OnPacketReceived(object sender, MavLinkPacket packet)
        {
            Console.WriteLine("Packet received MessageId " + packet.MessageId);   
        }
        // static void OnReceptionEnded(object? sender, EventArgs e)
        // {
        //     Console.WriteLine("Packet received");   
        // }
        public static uint DateTimeToUInt32Timestamp(DateTime dateTime)
        {
            // Unix epoch starts at 1970-01-01 00:00:00 UTC
            DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    
            // Calculate the time difference and get total seconds
            TimeSpan timeSpan = dateTime.ToUniversalTime() - epoch;
    
            // Convert to uint (throws an exception for negative values or overflow)
            // The maximum value for a UInt32 Unix timestamp is in the year 2106
            return (uint)timeSpan.TotalSeconds; 
        }
    }
}