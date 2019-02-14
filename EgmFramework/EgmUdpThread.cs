using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace EgmFramework
{
    public class EgmUdpThread : IEgmUdpThread
    {
        private int portNbr;
        private int sleepTimeDefault;
        private int timeout;
        private int seqNbr = 0;
        private bool exitThread = false;
        private Thread thread;

        public EgmUdpThread(int portNbr, int sleepTimeDefault, int timeout)
        {
            this.portNbr = portNbr;
            this.sleepTimeDefault = sleepTimeDefault;
            this.timeout = timeout;
        }

        public void StartUdp(IEgmMonitor monitor)
        {
            exitThread = false;
            thread = new Thread(() => StartUdpThread(monitor));
            thread.Start();
        }

        public void StartUdpThread(IEgmMonitor monitor)
        {
            //Debug.WriteLine($"Thread on port: {portNbr} started.");
            seqNbr = 0;
            int sleepTime = sleepTimeDefault;
            int timeoutCounter = 0;
            UdpClient udpClient = new UdpClient(portNbr);
            IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, portNbr);
            while (!exitThread)
            {
                byte[] data = null;
                try
                {
                    if (udpClient.Available > 0)
                    {
                        //Debug.WriteLine($"Thread on port: {portNbr} got data.");
                        sleepTime = 0;
                        data = udpClient.Receive(ref remoteEP);
                    }
                    else
                    {
                        sleepTime = sleepTimeDefault;
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
                    monitor.Write(portNbr, data);
                    timeoutCounter = 0;
                    seqNbr++;
                    // Send the message
                    byte[] datagram = monitor.Read(portNbr);
                    if(datagram != null)
                    {
                        var byteSent = udpClient.SendAsync(datagram, datagram.Length, remoteEP);
                    }
                }
                else if (seqNbr != 0 && timeoutCounter > timeout)
                {
                    Stop();
                }
                Thread.Sleep(sleepTime);
                sleepTime = sleepTimeDefault;
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
