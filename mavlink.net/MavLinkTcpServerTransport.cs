using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Concurrent;

namespace MavLinkNet
{
    public class MavLinkTcpServerTransport : MavLinkGenericTransport
    {
        public int ListeningPort = 4560;
        public int HeartBeatUpdateRateMs = 1000;

        private TcpListener mListener;
        private TcpClient mClient;
        private NetworkStream mStream;

        private ConcurrentQueue<byte[]> mReceiveQueue = new();
        private ConcurrentQueue<UasMessage> mSendQueue = new();

        private AutoResetEvent mReceiveSignal = new(true);
        private AutoResetEvent mSendSignal = new(true);

        private MavLinkAsyncWalker mMavLink = new();
        private bool mIsActive = true;
        private byte sequenceNumber = 0;

        // =========================================================
        // INIT / DISPOSE
        // =========================================================

        public override void Initialize()
        {
            InitializeMavLink();
            InitializeTcpServer();

            ThreadPool.QueueUserWorkItem(ProcessReceiveQueue);
            ThreadPool.QueueUserWorkItem(ProcessSendQueue);
        }

        public override void Dispose()
        {
            mIsActive = false;

            mStream?.Close();
            mClient?.Close();
            mListener?.Stop();

            mReceiveSignal.Set();
            mSendSignal.Set();
        }

        private void InitializeMavLink()
        {
            mMavLink.PacketReceived += HandlePacketReceived;
        }

        // =========================================================
        // TCP SERVER
        // =========================================================

        private void InitializeTcpServer()
        {
            mListener = new TcpListener(IPAddress.Any, ListeningPort);
            mListener.Start();

            Console.WriteLine($"[TCP] Listening on port {ListeningPort}");

            ThreadPool.QueueUserWorkItem(AcceptClientLoop);
        }

        private void AcceptClientLoop(object state)
        {
            while (mIsActive)
            {
                try
                {
                    mClient = mListener.AcceptTcpClient();
                    mStream = mClient.GetStream();

                    Console.WriteLine("[TCP] Client connected");

                    ThreadPool.QueueUserWorkItem(TcpReceiveLoop);
                }
                catch
                {
                    Console.WriteLine("Sleep 0.2 secs client");
                    Thread.Sleep(200);
                }
            }
        }

        // =========================================================
        // RECEIVE
        // =========================================================

        private void TcpReceiveLoop(object state)
        {
            Console.WriteLine("[TCP] Receiving data");
            
            byte[] buffer = new byte[4096];

            try
            {
                while (mIsActive && mClient.Connected)
                {
                    int read = mStream.Read(buffer, 0, buffer.Length);
                    if (read <= 0)
                        break;
            
                    byte[] data = new byte[read];
                    Buffer.BlockCopy(buffer, 0, data, 0, read);
                    mReceiveQueue.Enqueue(data);
                    mReceiveSignal.Set();
                }
            }
            catch (SocketException)
            {
                Console.WriteLine("[TCP] RX socket closed");
            }
        }

        private void ProcessReceiveQueue(object state)
        {
            while (mIsActive)
            {
                if (mReceiveQueue.TryDequeue(out var buffer))
                {
                    mMavLink.ProcessReceivedBytes(buffer, 0, buffer.Length);
                }
                else
                {
                    mReceiveSignal.WaitOne();
                }
            }

            HandleReceptionEnded(this);
        }

        // =========================================================
        // SEND
        // =========================================================

        private void ProcessSendQueue(object state)
        {
            while (mIsActive)
            {
                if (mSendQueue.TryDequeue(out var msg))
                {
                    SendMavlinkMessage(msg);
                }
                else
                {
                    mSendSignal.WaitOne();
                }
            }
        }

        private void SendMavlinkMessage(UasMessage msg)
        {
            if (mStream == null || !mClient.Connected)
                return;

            byte[] buffer = mMavLink.SerializeMessage(msg, true, sequenceNumber++);
            mStream.Write(buffer, 0, buffer.Length);
        }

        // =========================================================
        // HEARTBEAT
        // =========================================================

        public void BeginHeartBeatLoop()
        {
            ThreadPool.QueueUserWorkItem(HeartBeatLoop);
        }

        private void HeartBeatLoop(object state)
        {
            while (mIsActive)
            {
                foreach (UasMessage m in UavState.GetHeartBeatObjects())
                {
                    SendMessage(m);
                }

                Thread.Sleep(HeartBeatUpdateRateMs);
            }
        }

        // =========================================================
        // API
        // =========================================================

        public override void SendMessage(UasMessage msg)
        {
            mSendQueue.Enqueue(msg);
            mSendSignal.Set();
        }
    }
}
