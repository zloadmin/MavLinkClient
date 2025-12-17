using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MavLinkNet
{
    public abstract class MavLinkGenericTransport: IDisposable
    {
        public byte MavlinkSystemId = 255;
        public byte MavlinkComponentId = 0x00;
        public byte PacketSignalByte = byte.MinValue;
        public MavLinkState UavState = new MavLinkState();

        public event PacketReceivedDelegate OnPacketReceived;
        public event EventHandler OnReceptionEnded;

        public abstract void Initialize();
        public abstract void Dispose();
        public abstract void SendMessage(UasMessage msg);
        //public abstract void SendRawPacket(MavLinkPacket packet);


        // __ MavLink events __________________________________________________


        protected void HandlePacketReceived(object sender, MavLinkPacketBase e)
        {
            if (OnPacketReceived != null) OnPacketReceived(sender, e);
        }

        protected void HandleReceptionEnded(object sender)
        {
            if (OnReceptionEnded != null) OnReceptionEnded(sender, EventArgs.Empty);
        }
    }
}
