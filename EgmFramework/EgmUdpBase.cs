using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace EgmFramework
{
    public abstract class EgmUdpBase : IEgmUdpBase
    {
        private static int _portNbr;
        private static int _mType;
        private static int _sleepTimeDefault;
        private static int _timeout;
        private int seqNbr = 0;
        private bool exitThread = false;
        private Thread thread;

        public EgmUdpBase(int portNbr, int mType, int sleepTimeDefault, int timeout)
        {
            _portNbr = portNbr;
            _mType = mType;
            _sleepTimeDefault = sleepTimeDefault;
            _timeout = timeout;
        }

        public void StartUdp(IEgmMonitor monitor)
        {
            exitThread = false;
            thread = new Thread(() => StartUdpThread(monitor));
            thread.Start();
        }

        public void StartUdpThread(IEgmMonitor monitor)
        {
            seqNbr = 0;
            int sleepTime = _sleepTimeDefault;
            int timeoutCounter = 0;
            UdpClient udpClient = new UdpClient(_portNbr);
            IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, _portNbr);
            while (!exitThread)
            {
                byte[] data = null;
                try
                {
                    if (udpClient.Available > 0)
                    {
                        sleepTime = 0;
                        data = udpClient.Receive(ref remoteEP);
                    }
                    else
                    {
                        sleepTime = _sleepTimeDefault;
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.StackTrace);
                    Stop();
                }
                if (data != null)
                {
                    //ProcessData(udpClient, remoteEP, data, monitor);
                    monitor.Write(_portNbr, _mType, data);
                    timeoutCounter = 0;
                    seqNbr++;
                    // Send the message
                    byte[] datagram = monitor.Read(_portNbr, _mType);
                    if(datagram != null)
                    {
                        var byteSent = udpClient.SendAsync(datagram, datagram.Length, remoteEP);
                    }
                }
                else if (seqNbr != 0 && timeoutCounter > _timeout)
                {
                    Stop();
                }
                Thread.Sleep(sleepTime);
                sleepTime = _sleepTimeDefault;
                timeoutCounter++;
            }
        }

        public void Stop()
        {
            exitThread = true;
            thread = null;
        }
    }
}
