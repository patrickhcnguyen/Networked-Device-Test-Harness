/*
A TCP server listens on a port, accepts client connections, read bytes, and writes bytes back
This program is entry point and lifecycle owner, and is responsible for starting the listener, accepting clients, and handing connections to the handler
*/
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using DeviceSimulator.Networking;
using DeviceSimulator.Handling;
using DeviceSimulator.State;
namespace DeviceSimulator
{
    class FakeDeviceServer
    {
        static async Task Main(string[] args) {
            // i need to first setup a tcp listener to start the server and start accepting clients, which means i need to open a port, listen, 
            // and accept connections
            Int32 port = 13000;
            IPAddress localAddr = IPAddress.Parse("127.0.0.1");

            TcpListener server = new TcpListener(localAddr, port);
            // start the server and start listening for clients
            server.Start();
            Console.WriteLine("Server is running on port {0}", port);

            while (true) {
                Console.WriteLine("Waiting for a connection...");
                // TcpClient client = await server.AcceptTcpClientAsync();
                var state = new DeviceState();
                var handler = new CommandHandler(state);

                while (true) {
                    TcpClient client = await server.AcceptTcpClientAsync();
                    _ = new ClientConnection(client, handler).RunAsync();
                }
            }
        }
    }
}

