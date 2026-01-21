namespace MavLinkNet;

public class FlyDron
{
    private static MavLinkUdpTransport _udpTransport;
    // private static MavLinkTcpTransport _tcpTransport;

    static void Run()
    {
        InitConnection();
        
        // controls. 
        while (true)
        {
            _udpTransport.SendMessage(GetDisArmMessage());
            _udpTransport.SendMessage(GetOffboardModeMessage());
            _udpTransport.SendMessage(GetTakeOffMessage());
            // _udpTransport.SendMessage(GetUasHilActuatorControls());
            Thread.Sleep(100);
        }
    }

    private static void InitConnection()
    {
        _udpTransport = new MavLinkUdpTransport();
        _udpTransport.OnPacketReceived += OnPacketReceived;
        _udpTransport.Initialize();
        _udpTransport.HeartBeatUpdateRateMs = 100;
        _udpTransport.BeginHeartBeatLoop();
    }


    private static UasHilActuatorControls GetUasHilActuatorControls()
    {
        UasHilActuatorControls controls = new UasHilActuatorControls();
        controls.TimeUsec = (ulong)Environment.TickCount64 * 1000;
        controls.Controls[0] = 0.0f; // roll
        controls.Controls[1] = 0.0f; // pitch
        controls.Controls[2] = 0.0f; // yaw
        controls.Controls[3] = 0.6f; // throttle
        controls.Flags = 0;
        controls.Mode = MavMode.Preflight;
        controls.SystemId = 1;
        return controls;
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
        Console.WriteLine("Packet received " + packet.MessageId);
        // switch(packet.MessageId) {
        //     case 31:
        //         PrintUasAttitudeQuaternion((UasAttitudeQuaternion)packet.Message);
        //     break;
        //
        // }
    }

    private static void PrintUasAttitudeQuaternion(UasAttitudeQuaternion message)
    {
        Console.Clear();
        Console.WriteLine("TimeBootMs: " + message.TimeBootMs);
        Console.WriteLine("Q1: " + message.Q1);
        Console.WriteLine("Q2: " + message.Q2);
        Console.WriteLine("Q3: " + message.Q3);
        Console.WriteLine("Q4: " + message.Q4);
        Console.WriteLine("Rollspeed: " + message.Rollspeed);
        Console.WriteLine("Pitchspeed: " + message.Pitchspeed);
        Console.WriteLine("Yawspeed: " + message.Yawspeed);
    }
}