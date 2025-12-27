/*
The MIT License (MIT)

Copyright (c) 2013, David Suarez

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/
using System;

namespace MavLinkNet
{
  

public class MavlinkMessage
{
    public enum MavlinkVersion
    {
        V1,
        V2
    }

    public MavlinkVersion Version { get; private set; }

    public byte Magic { get; private set; }
    public byte PayloadLength { get; private set; }
    public byte Sequence { get; private set; }
    public byte SystemId { get; private set; }
    public byte ComponentId { get; private set; }
    public uint MessageId { get; private set; }

    public byte[] Payload { get; private set; } = Array.Empty<byte>();
    public ushort Crc { get; private set; }

    public bool IsSigned { get; private set; }
    public byte[]? Signature { get; private set; }

    // ================================
    // Deserialize
    // ================================
    public static MavlinkMessage Deserialize(byte[] packet)
    {
        if (packet == null || packet.Length < 6)
            throw new ArgumentException("Packet too short");

        var msg = new MavlinkMessage();
        int index = 0;

        msg.Magic = packet[index++];

        // ---------------- MAVLink v1 ----------------
        if (msg.Magic == 0xFE)
        {
            msg.Version = MavlinkVersion.V1;

            msg.PayloadLength = packet[index++];
            msg.Sequence = packet[index++];
            msg.SystemId = packet[index++];
            msg.ComponentId = packet[index++];
            msg.MessageId = packet[index++];

            int payloadLen = msg.PayloadLength;
            int expectedLen = 6 + payloadLen + 2;

            if (packet.Length < expectedLen)
                throw new ArgumentException("Invalid MAVLink v1 length");

            msg.Payload = new byte[payloadLen];
            Array.Copy(packet, index, msg.Payload, 0, payloadLen);
            index += payloadLen;

            msg.Crc = BitConverter.ToUInt16(packet, index);
        }

        // ---------------- MAVLink v2 ----------------
        else if (msg.Magic == 0xFD)
        {
            msg.Version = MavlinkVersion.V2;

            msg.PayloadLength = packet[index++];
            byte incompatFlags = packet[index++];
            byte compatFlags = packet[index++];

            msg.Sequence = packet[index++];
            msg.SystemId = packet[index++];
            msg.ComponentId = packet[index++];

            msg.MessageId =
                (uint)(packet[index] |
                (packet[index + 1] << 8) |
                (packet[index + 2] << 16));
            index += 3;

            msg.IsSigned = (incompatFlags & 0x01) != 0;

            int payloadLen = msg.PayloadLength;
            msg.Payload = new byte[payloadLen];
            Array.Copy(packet, index, msg.Payload, 0, payloadLen);
            index += payloadLen;

            msg.Crc = BitConverter.ToUInt16(packet, index);
            index += 2;

            if (msg.IsSigned)
            {
                msg.Signature = new byte[13];
                Array.Copy(packet, index, msg.Signature, 0, 13);
            }
        }
        else
        {
            throw new ArgumentException($"Unknown MAVLink magic: 0x{msg.Magic:X2}");
        }

        return msg;
    }
}

}
