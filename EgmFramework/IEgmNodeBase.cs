using System;
using System.Collections.Generic;
using System.Text;

namespace EgmFramework
{
    public interface IEgmNodeBase
    {
        /// <summary>
        /// Instantiate and start all EgmUdpBase and EgmMonitor objects in this EgmNode.
        /// </summary>
        void StartNode();

        /// <summary>
        /// Stop and clean up all  EgmUdpBase and EgmMonitor objects in this EgmNode.
        /// </summary>
        void StopNode();
    }
}
