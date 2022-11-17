using System.Net;
using System.Net.Sockets;

namespace Phipseyy.Common.Modules;

public static class Helpers
{
    public static int FreeTcpPort()
    {
        var l = new TcpListener(IPAddress.Loopback, 0);
        l.Start();
        var port = ((IPEndPoint)l.LocalEndpoint).Port;
        l.Stop();
        return port;
    }
}