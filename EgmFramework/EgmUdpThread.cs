using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace EgmFramework
{
    /// <summary>
    /// This is an implementation of the IEgmUdpThread interface. The purpose of this class 
    /// is to act as a wrapper for a worker thread that handles UDP communication for EGM. 
    /// In this implementation, the worker thread will periodically check the udp buffer for 
    /// data to write to its monitor and then try to reply to the client that last sent it 
    /// a message with data it reads for its monitor. If there is data in the buffer, the 
    /// worker thread not sleep before trying to read from its udp buffer again. If there is 
    /// no data in the udp buffer, the thread will sleep for a number of miliseconds specified 
    /// at the creation of the class. 
    /// </summary>
    /// <seealso cref="EgmFramework.IEgmUdpThread"/>
    public class EgmUdpThread : IEgmUdpThread
    {
        private int portNbr;
        private int sleepTimeDefault;
        private int timeout;
        private int seqNbr = 0;
        private bool exitThread = false;
        private Thread thread;

        /// <summary>
        /// To create an instance of the EgmUdpThread class: assign it a port, define the 
        /// default sleep time in ms, and the timeout in ms.
        /// </summary>
        /// <param name="portNbr"></param>
        /// <param name="sleepTimeDefault"></param>
        /// <param name="timeout"></param>
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
