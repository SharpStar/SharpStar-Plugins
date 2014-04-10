using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XSockets.Core.Common.Socket;

namespace SharpStarWebPlugin
{
    public class SharpStarWeb
    {

        private IXSocketServerContainer _container;

        public void Start()
        {

            _container = XSockets.Plugin.Framework.Composable.GetExport<IXSocketServerContainer>();

            _container.StartServers();

        }

        public void Stop()
        {
            _container.StopServers();
            _container.Dispose();
        }

    }
}
