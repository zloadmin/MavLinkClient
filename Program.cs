using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Diagnostics;

namespace MavLinkNet
{
    internal class Program
    {
        private static MavLinkUdpTransport _udpTransport;

        static void Main()
        {
            InitConnection();

            Thread.Sleep(1000);
            
            
            var sw = Stopwatch.StartNew();
            while (sw.Elapsed < TimeSpan.FromSeconds(5))
            {
                _udpTransport.SendMessage(GetDisArmMessage());
                _udpTransport.SendMessage(GetOffboardModeMessage());
                _udpTransport.SendMessage(GetTakeOffMessage());
                Thread.Sleep(100);
            }
            Thread.Sleep(3000);
            _udpTransport.SendMessage(GetLandMessage());
            Thread.Sleep(1000);
            
        }

        private static void InitConnection()
        {
            _udpTransport = new MavLinkUdpTransport();
            // udpTransport.OnPacketReceived += OnPacketReceived;
            _udpTransport.Initialize();
            _udpTransport.HeartBeatUpdateRateMs = 100;
            _udpTransport.BeginHeartBeatLoop();
        }

        private static UasCommandLong GetDisArmMessage()
        {
            return new UasCommandLong
            {
                Param1 = 1,
                Param2 = 0,
                Param3 = 0,
                Param4 = 0,
                Param5 = 0,
                Param6 = 0,
                Param7 = 0,
                Command = MavCmd.ComponentArmDisarm,
                TargetSystem = 255,
                TargetComponent = 0,
                Confirmation = 1
            };
        }
        private static UasCommandLong GetLandMessage()
        {
            return new UasCommandLong
            {
                Param1 = 0,
                Param2 = 0,
                Param3 = 0,
                Param4 = 0,
                Param5 = 0,
                Param6 = 0,
                Param7 = 0,
                Command = MavCmd.NavLand,
                TargetSystem = 255,
                TargetComponent = 0,
                Confirmation = 1
            };
        }

        private static UasSetMode GetOffboardModeMessage()
        {
            return new UasSetMode
            {
                CustomMode = 0x060000,
                TargetSystem = 0x01,
                BaseMode = 0x01,
                SystemId = 0x01,
                ComponentId = 0xF0, // Component ID (sender = 240)
            };
        }

        private static UasSetPositionTargetLocalNed GetTakeOffMessage()
        {
            return new UasSetPositionTargetLocalNed
            {
                TimeBootMs = unchecked((uint)Environment.TickCount),
                X = 0.0f,
                Y = 0.0f,
                Z = -1.0f,
                Vx = 0.0f,
                Vy = 0.0f,
                Vz = 0.0f,
                Afx = 0.0f,
                Afy = 0.0f,
                Afz = 0.0f,
                Yaw = 0.0f,
                YawRate = 0.0f,
                TypeMask = 0x09F8, // ignore velocity, accel, yaw_rate
                TargetSystem = 0x01,
                TargetComponent = 0x01,
                CoordinateFrame = MavFrame.LocalNed
            };
        }
        private static UasSetPositionTargetLocalNed GetFlyForwardMessage()
        {
            return new UasSetPositionTargetLocalNed
            {
                TimeBootMs = unchecked((uint)Environment.TickCount),
                X = 1.0f,
                Y = 0.0f,
                Z = -1.0f,
                Vx = 0.0f,
                Vy = 0.0f,
                Vz = 0.0f,
                Afx = 0.0f,
                Afy = 0.0f,
                Afz = 0.0f,
                Yaw = 0.0f,
                YawRate = 0.0f,
                TypeMask = 0x09F8, // ignore velocity, accel, yaw_rate
                TargetSystem = 0x01,
                TargetComponent = 0x01,
                CoordinateFrame = MavFrame.LocalNed
            };
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