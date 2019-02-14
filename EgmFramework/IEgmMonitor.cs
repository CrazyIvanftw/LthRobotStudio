using System;
using System.Collections.Generic;
using System.Text;

namespace EgmFramework
{
    public interface IEgmMonitor
    {
        /// <summary>
        /// When an EgmUdpBase instance receives data, it handles it by calling the Write method and passing the EgmMonitor 
        /// the port numbr of the EgmUdpBase thread, an int that corresponds to the message type (can be used or not), and 
        /// the raw byte array of the message. It is the job of the EgmMonitor to de-serialize and handle the message that is passed to it.
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
