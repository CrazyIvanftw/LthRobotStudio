using System;
using System.Collections.Generic;
using System.Text;

namespace EgmFramework
{

    /// <summary>
    /// Any implementation of the IEgmUdpBase interface is considered to be a wrapper for a worker thread that handled udp comminication. The worker threads have to 
    /// be wrapped like this so they can be safely started and stopped even from within the RobotStudio simulation environment. Calling the StartUdp method will create 
    /// a worker thread using the StartUdpThread method. Calling the Stop method will cause the worker thread to exit and return safely, then it will remove references 
    /// to the thread so it will be cleaned up correctly.
    /// </summary>
    public interface IEgmUdpBase
    {
        /// <summary>
        /// Starts the EgmUdpBase by instantiating and starting a worker thread that handles the udp receive and send functions.
        /// </summary>
        /// <param name="monitor"></param>
        void StartUdp(IEgmMonitor monitor);

        /// <summary>
        /// This is the method that is called to start the worker thread
        /// </summary>
        /// <param name="monitor"></param>
        void StartUdpThread(IEgmMonitor monitor);

        /// <summary>
        /// Cause the worker thread to stop execution and to return, thereby exiting the thread safely. 
        /// </summary>
        void Stop();
    }
}
