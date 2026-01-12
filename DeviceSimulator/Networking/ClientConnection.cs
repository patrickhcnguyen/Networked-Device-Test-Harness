/*
How do raw TCP bytes turn into complete messages?
This is a file meant for streams, partial reads, delimiters, and assembling messages safely
It's basically for TCP stream -> complete messages

This file will:
- store a TcpClient
- get a NetworkStream
- read bytes in a loop
- append decoded text to a buffer
- extract completed lines
- emit those lines upward
*/

// readasync Asynchronously reads a sequence of bytes from the current stream and advances the position within the stream by the number of bytes read.
// this means we might get partial messages, multiple messages, or half a line, which is why we need to buffer the data
// one ReadAsync is NOT one message
// we can use StringBuilder to accumulate text, remove processed parts, and keep leftovers for the next read

// writeasync(byte[], int32, int32) writes a sequence of bytes to the curr stream and advances the curr pos within this stream by the number of bytes written
// a networkstream is a continuous stream of bytes not messages
using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Text;
namespace DeviceSimulator.Networking {
    /*
    - Responsible for:
        - Managing a single TCP client connection
        - Reading bytes from the network stream
        - Buffering partial data
        - Emitting complete newline-delimited messages
    */
    class ClientConnection {
        private readonly TcpClient _client;
        private readonly StringBuilder _buffer = new StringBuilder();
        public ClientConnection(TcpClient client) {
            _client = client;
        }
        // entry point for handling this connection
        public async Task RunAsync() {
            try {
                // get into the network stream
                NetworkStream stream = _client.GetStream();
                byte[] buffer = new byte[256];
                int bytesRead;

                while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) != 0) {
                    Console.WriteLine($"Received {bytesRead} bytes");
                }
            } catch (Exception ex) {
                Console.WriteLine($"Error: {ex.Message}");
            } finally {
                Close();
            }
        }
        // reads raw bytes from the network stream
        private Task<int> ReadFromStreamAsync() {
            return Task.FromResult(0);
        }
        // appends newly receieved data to an internal transfer
        private void AppendToBuffer(string data) {

        }
        // attempts to extract one complete message from the buffer, messages are newline-delimited
        private bool TryGetNextMessage(out string message) {
            message = string.Empty;
            // return true if a message was found
            return false;
        }
        // handles a fully received message, but for now we just log it
        private Task HandleMessageAsync(string message) {
            // for now just log it, later forward to the message handler
            return Task.CompletedTask;
        }
        // clean up resources
        private void Close() {
            _buffer.Clear();
            _client.Close();
        }
    }
}