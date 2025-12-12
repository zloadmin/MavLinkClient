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
            Console.WriteLine("test");
            udpTransport = new MavLinkUdpTransport();
            udpTransport.Initialize();

            int Iteration = 0;
            while (true)
            {
                UasStatustext statustext = new UasStatustext();
                statustext.Text = getStringAsFiftyByteArray("Iteration " + Iteration);
                udpTransport.SendMessage(statustext);
                Thread.Sleep(1000);
                Iteration++;
            }
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

        // static void OnPacketReceived(object sender, MavLinkPacket packet)
        // {
        //     Console.WriteLine("Packet received");   
        // }
        // static void OnReceptionEnded(object? sender, EventArgs e)
        // {
        //     Console.WriteLine("Packet received");   
        // }
    }
}