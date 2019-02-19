using System;
using System.Collections.Generic;
using System.Text;

namespace EgmFramework
{
    /// <summary>
    /// Implementations of IEgmMonitor are intended to contain the logic of the specific 
    /// EGM system being constructed. The idea here is that an EgmUdpThread will receive 
    /// the raw data (as a byte[] from a sensor or an Egm process and simple call the 
    /// Write(int udpPortNbr, byte[] data) method of the monitor. The EgmUdpThread will 
    /// then call the Read(int udpPortNbr) in order to get the raw data that it will send 
    /// as a response. In the case of an EGM endpoint, the de-serialisation of the egm.proto
    /// files will happen in the monitor. If the system being build has several nodes, the 
    /// logic of what to do with what kind of message can be managed by the portNbrs. I.e. 
    /// if the EGM process is communicating on port 6510 and a sensor is communicating on port 
    /// 8080, the logic in the monitor can be handeled with switch cases on the port number. 
    /// </summary>
    /// <seealso cref="EgmFramework.DemoEgmMonitor"/>
    public interface IEgmMonitor
    {
        /// <summary>
        /// When an EgmUdpBase instance receives data, it handles it by calling the 
        /// Write method and passing the EgmMonitor the port numbr of the 
        /// EgmUdpBase thread, an int that corresponds to the message type 
        /// (can be used or not), and the raw byte array of the message. 
        /// It is the job of the EgmMonitor to de-serialize and handle the 
        /// message that is passed to it.
        /// </summary>
        /// <param name="udpPortNbr"></param>
        /// <param name="messageType"></param>
        /// <param name="data"></param>
        void Write(int udpPortNbr, byte[] data);

        /// <summary>
        /// When an EgmUdpBase instance needs to send a message, it calles the Read method of the EgmMonitor. It is the job of the EgmMonitor 
        /// to pass the correct message to the EgmUdpBase.
        /// </summary>
        /// <param name="udpPortNbr"></param>
        /// <param name="messageType"></param>
        /// <returns></returns>
        byte[] Read(int udpPortNbr);
    }
}
