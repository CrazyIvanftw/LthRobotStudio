using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace EgmFramework
{
    public abstract class EgmNodeBase : IEgmNodeBase
    {
        public void StartNode()
        {
            throw new NotImplementedException();
        }

        public void StopNode()
        {
            throw new NotImplementedException();
        }
    }
}
