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
            // _udpTransport.OnPacketReceived += OnPacketReceived;
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
                BaseMode = MavMode.Custom,
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
                TypeMask = PositionTargetTypemask.CustomIgnore, // ignore velocity, accel, yaw_rate
                TargetSystem = 0x01,
                TargetComponent = 0x01,
                CoordinateFrame = MavFrame.LocalNed
            };
        }
        
        
        static void OnPacketReceived(object sender, MavLinkPacketBase packet)
        {
              
        }

        
    }
}