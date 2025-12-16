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
            udpTransport.HeartBeatUpdateRateMs = 100;
            udpTransport.BeginHeartBeatLoop();
            // UasHeartbeat heartbeat = new UasHeartbeat();


            while (true)
            {
                UasCommandLong arm = new UasCommandLong();
                arm.Param1 = 1;
                arm.Param2 = 0;
                arm.Param3 = 0;
                arm.Param4 = 0;
                arm.Param5 = 0;
                arm.Param6 = 0;
                arm.Param7 = 0;
                arm.Command = MavCmd.ComponentArmDisarm;
                arm.TargetSystem = 255;
                arm.TargetComponent = 0;
                arm.Confirmation = 1;
                udpTransport.SendMessage(arm);
                Thread.Sleep(1000);
            }
            
        }

        static char[] getStringAsFiftyByteArray(string source)
        {
            char[] buffer = new char[50];
            int i = 0;
            foreach (char c in source)
            {
                buffer[i++] = c;
            }

            return buffer;
        }

        static void OnPacketReceived(object sender, MavLinkPacket packet)
        {
             
            // UasHeartbeat Heartbeat = (UasHeartbeat)packet.Message;
            // Heartbeat.Type = MavType.Gcs;
            // Heartbeat.Autopilot = MavAutopilot.Generic;
            // Heartbeat.BaseMode = 0;
            // Heartbeat.CustomMode = 0;
            // Heartbeat.SystemStatus = MavState.Active;
            // Heartbeat.MavlinkVersion = (byte)3;
            
            // Heartbeat.BaseMode = MavModeFlag.CustomModeEnabled;
            // Heartbeat.Autopilot = MavAutopilot.Invalid;
            // Heartbeat.
            // Console.Write("Heartbeat.Autopilot" + Heartbeat.Autopilot.ToString());
            // Console.Write("Heartbeat.BaseMode" + Heartbeat.BaseMode.ToString());
            // Console.Write("Heartbeat.CustomMode" + Heartbeat.CustomMode.ToString());
            // Console.Write("Heartbeat.MavlinkVersion" + Heartbeat.MavlinkVersion.ToString());
            // Console.Write("Heartbeat.SystemStatus" + Heartbeat.SystemStatus.ToString());
            // Console.Write("Heartbeat.Type" + Heartbeat.Type.ToString());
            // Console.Write("Heartbeat.SystemId" + packet.SystemId);
            // Console.Write("Heartbeat.ComponentId" + packet.ComponentId);
            //
            // UasSysStatus SysStatus = (UasSysStatus)packet.Message;
            // Console.WriteLine("OnboardControlSensorsPresent " + SysStatus.OnboardControlSensorsPresent.ToString());
            // Console.WriteLine("OnboardControlSensorsEnabled " + SysStatus.OnboardControlSensorsEnabled.ToString());
            // Console.WriteLine("OnboardControlSensorsHealth " + SysStatus.OnboardControlSensorsHealth.ToString());
            // Console.WriteLine("Load " + SysStatus.Load.ToString());
            // Console.WriteLine("VoltageBattery " + SysStatus.VoltageBattery.ToString());
            // Console.WriteLine("CurrentBattery " + SysStatus.CurrentBattery.ToString());
            // Console.WriteLine("DropRateComm " + SysStatus.DropRateComm.ToString());
            // Console.WriteLine("ErrorsComm " + SysStatus.ErrorsComm.ToString());
            // Console.WriteLine("ErrorsCount1 " + SysStatus.ErrorsCount1.ToString());
            // Console.WriteLine("ErrorsCount2 " + SysStatus.ErrorsCount2.ToString());
            // Console.WriteLine("ErrorsCount3 " + SysStatus.ErrorsCount3.ToString());
            // Console.WriteLine("ErrorsCount4 " + SysStatus.ErrorsCount4.ToString());
            // Console.WriteLine("BatteryRemaining " + SysStatus.BatteryRemaining.ToString());

            // Console.WriteLine("Packet received MessageId " + packet.Message.ToString());   
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