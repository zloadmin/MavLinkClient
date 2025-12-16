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
using System.IO;
using System.Text;

namespace MavLinkNet
{
    public class MavLinkPacket
    {
        public const int PacketOverheadNumBytes = 10;

        public bool IsValid = false;

        public byte PayLoadLength;
        public byte IncompatibilityFlags;
        public byte CompatibilityFlags;
        public byte PacketSequenceNumber;
        public byte SystemId;
        public byte ComponentId;
        public uint MessageId;
        public byte[] Payload;
        // public byte[] MessageIdBytes;
        public byte Checksum1;
        public byte Checksum2;

        public UasMessage Message;

        // __ Deserialization _________________________________________________

        /*
         * Byte order:
         * 
         * 0  Packet start sign	
         * 1	 Payload length	 0 - 255
         * 2	 Packet sequence	 0 - 255
         * 3	 System ID	 1 - 255
         * 4	 Component ID	 0 - 255
         * 5	 Message ID	 0 - 255
         * 6 to (n+6)	 Data	 (0 - 255) bytes
         * (n+7) to (n+8)	 Checksum (high byte, low byte) for v0.9, lowbyte, highbyte for 1.0
         *
         */
        public static MavLinkPacket Deserialize(BinaryReader s, byte payloadLength)
        {
            MavLinkPacket result = new MavLinkPacket()
            {
                PayLoadLength = (payloadLength == 0) ? s.ReadByte() : payloadLength,
                IncompatibilityFlags = s.ReadByte(),
                CompatibilityFlags = s.ReadByte(),
                PacketSequenceNumber = s.ReadByte(),
                SystemId = s.ReadByte(),
                ComponentId = s.ReadByte(),
                MessageId = BitConverter.ToUInt32(PadRightByteArray(s.ReadBytes(3)), 0),
                // MessageIdBytes = s.ReadBytes(3),
            };

            // result.MessageId = BitConverter.ToUInt32(PadRightByteArray(result.MessageIdBytes), 0);
            // Read the payload instead of deserializing so we can validate CRC.
            result.Payload = s.ReadBytes(result.PayLoadLength);
            result.Checksum1 = s.ReadByte();
            result.Checksum2 = s.ReadByte();

            if (result.IsValidCrc())
            {
                result.DeserializeMessage();
            } else {
                // Console.WriteLine("NOT VALID CRC message ID" + result.MessageId);
            }

            return result;
        }
        
        private static byte[] PadRightByteArray(byte[] sourceArray)
        {
            var newArray = new byte[3 + 1];
            sourceArray.CopyTo(newArray, 0);
            newArray[3] = 0;
            return newArray;
        }

        public int GetPacketSize()
        {
            return PacketOverheadNumBytes + PayLoadLength;
        }

        private bool IsValidCrc()
        {
            UInt16 crc = GetPacketCrc(this);

            return ( ((byte)(crc & 0xFF) == Checksum1) &&
                     ((byte)(crc >> 8) == Checksum2) );
        }

        private void DeserializeMessage()
        {
            // string hex = BitConverter.ToString(MessageId);
            // Console.WriteLine("Message id HEX " + hex);
            // Console.WriteLine("Message in byte " + (byte)MessageId);
            // Console.WriteLine("Message in byte " + (byte)MessageId);
            
            // Console.WriteLine(MessageId.ToString("X"));
            
            
            UasMessage result = UasSummary.CreateFromId((byte)MessageId);
                        
            if (result == null) return;  // Unknown type

            // Zero-bytes are truncated at the end of the packet
            // We have to fill the payload in that case otherwise the binary ready will throw exception
            FillPayloadIfNeeded(result);

            using (MemoryStream ms = new MemoryStream(Payload))
            {
                using (BinaryReader br = GetBinaryReader(ms))
                {
                    result.DeserializeBody(br);
                }
            }

            Message = result;
            IsValid = true;
        }
        
        
        private void FillPayloadIfNeeded(UasMessage message)
        {
            // Console.WriteLine("FillPayloadIfNeeded");
            if(Payload.Length < message.PacketSize)
            {
                var newArray = new byte[message.PacketSize];
                Payload.CopyTo(newArray, 0);
                Payload = newArray;
            }
        }

        public static BinaryReader GetBinaryReader(Stream s)
        {
            return new BinaryReader(s, Encoding.ASCII);
        }


        // __ Serialization ___________________________________________________


        public static MavLinkPacket GetPacketForMessage(
            UasMessage msg, byte sequenceNumber)
        {
            MavLinkPacket result = new MavLinkPacket()
            {
                SystemId = msg.SystemId,
                ComponentId = msg.ComponentId,
                PacketSequenceNumber = sequenceNumber,
                MessageId = msg.MessageId,
                Message = msg
            };

            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    msg.SerializeBody(bw);
                }

                result.Payload = ms.ToArray();
                result.PayLoadLength = (byte)result.Payload.Length;
                result.UpdateCrc();
            }

            return result;
        }

        public static byte[] GetBytesForMessage(
            UasMessage msg, byte sequenceNumber, byte signalMark)
        {
            MavLinkPacket p = MavLinkPacket.GetPacketForMessage(
                msg, sequenceNumber);

            int bufferSize = p.GetPacketSize();

            if (signalMark != 0) bufferSize++;

            using (MemoryStream s = new MemoryStream())
            {
                using (BinaryWriter w = new BinaryWriter(s))
                {
                    if (signalMark != 0) w.Write(signalMark);
                    p.Serialize(w);
                }

                return s.ToArray();
            }
        }


        public void Serialize(BinaryWriter w)
        {
            byte[] fourBytes = BitConverter.GetBytes(MessageId);
            byte[] threeBytes = new byte[3];
            Array.Copy(fourBytes, 0, threeBytes, 0, 3);
            
            w.Write(PayLoadLength);
            w.Write(IncompatibilityFlags);
            w.Write(CompatibilityFlags);
            w.Write(PacketSequenceNumber);
            w.Write(SystemId);
            w.Write(ComponentId);
            w.Write(threeBytes);
            w.Write(Payload);
            w.Write(Checksum1);
            w.Write(Checksum2);
            // Payload
            string hex = BitConverter.ToString(Payload);
            // Console.WriteLine(hex);
        }

        private void UpdateCrc()
        {
            UInt16 crc = GetPacketCrc(this);
            Checksum1 = (byte)(crc & 0xFF);
            Checksum2 = (byte)(crc >> 8);
        }

        public static UInt16 GetPacketCrc(MavLinkPacket p)
        {
            UInt16 crc = X25CrcSeed;

            crc = X25CrcAccumulate(p.PayLoadLength, crc);
            crc = X25CrcAccumulate(p.IncompatibilityFlags, crc);
            crc = X25CrcAccumulate(p.CompatibilityFlags, crc);
            crc = X25CrcAccumulate(p.PacketSequenceNumber, crc);
            crc = X25CrcAccumulate(p.SystemId, crc);
            crc = X25CrcAccumulate(p.ComponentId, crc);
            crc = X25CrcAccumulate((byte)(p.MessageId & 0x0000FF), crc);
            crc = X25CrcAccumulate((byte)((p.MessageId & 0x00FF00) >> 8), crc);
            crc = X25CrcAccumulate((byte)((p.MessageId & 0xFF0000) >> 16), crc);

            for (int i = 0; i < p.Payload.Length; ++i)
            {
                crc = X25CrcAccumulate(p.Payload[i], crc);
            }

            crc = X25CrcAccumulate(UasSummary.GetCrcExtraForId((byte)p.MessageId), crc);

            return crc;
        }



        // __ CRC _____________________________________________________________


        // CRC code adapted from Mavlink C# generator (https://github.com/mavlink/mavlink)

        const UInt16 X25CrcSeed = 0xffff;

        public static UInt16 X25CrcAccumulate(byte b, UInt16 crc)
        {
            unchecked
            {
                byte ch = (byte)(b ^ (byte)(crc & 0x00ff));
                ch = (byte)(ch ^ (ch << 4));
                return (UInt16)((crc >> 8) ^ (ch << 8) ^ (ch << 3) ^ (ch >> 4));
            }
        }
    }
}
