/*
This is just a TestRunner to test the DeviceSimulator, it will:
- open a TCP connection to the DeviceSimulator
- send newline delimited commands
- read newline delimited responses
- compare actual responses to expected responses
- print pass/fail results
- exit with 0 if all tests pass, 1 if any test fails

for now it can just be a simple deterministic test, we don't need to worry about concurrency or async for now we just want to make sure it works

we want to verify that we can handle:
- 1 client
- 1 connection
- sequential commands
- deterministic assertions

later we can verify:
- n clients
- parallel commands
- no crashes
- no state variants
*/

using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;

namespace TestRunner
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var client = new TcpClient();
            await client.ConnectAsync(IPAddress.Loopback, 13000);

            bool passed = await FunctionalTest(client); // worry about concurrency later
            client.Close();

            if (!passed)
            {
                Console.WriteLine("At least one test failed");
                Environment.Exit(1);
            }
            Console.WriteLine("All tests passed");
            Environment.Exit(0);
        }

        public static async Task<bool> FunctionalTest(TcpClient client)
        {
            NetworkStream stream = client.GetStream();
            var reader = new StreamReader(stream, Encoding.UTF8);
            var writer = new StreamWriter(stream, new UTF8Encoding(false)) { AutoFlush = true };


            // 1. Test STATUS
            await writer.WriteLineAsync("STATUS");

            string response = await reader.ReadLineAsync();
            Console.WriteLine($"RAW RESPONSE: {response}");
            if (response != "OK INPUT=HDMI1 POWER=ON") {
                Console.WriteLine("Test failed: STATUS response mismatch");
                return false;
            }
            Console.WriteLine("--------------------------------");
            Console.WriteLine("[PASS]: STATUS default");
            Console.WriteLine("EXPECTED: OK INPUT=HDMI1 POWER=ON");
            Console.WriteLine("ACTUAL: {response}");
            Console.WriteLine("--------------------------------");

            // 2. Test SET_INPUT HDMI2
            await writer.WriteLineAsync("SET_INPUT HDMI2");

            response = await reader.ReadLineAsync();
            if (response != "OK")
            {
                Console.WriteLine("Test failed: SET_INPUT response mismatch");
                return false;
            }
            Console.WriteLine("--------------------------------");
            Console.WriteLine("[PASS]: SET_INPUT HDMI2");
            Console.WriteLine("EXPECTED: OK");
            Console.WriteLine("ACTUAL: {response}");
            Console.WriteLine("--------------------------------");
            // 3. Test STATUS after SET_INPUT
            await writer.WriteLineAsync("STATUS");
            response = await reader.ReadLineAsync();
            if (response != "OK INPUT=HDMI2 POWER=ON")
            {
                Console.WriteLine("Test failed: STATUS response mismatch after SET_INPUT");
                return false;
            }
            Console.WriteLine("--------------------------------");
            Console.WriteLine("[PASS]: STATUS after SET_INPUT");
            Console.WriteLine("EXPECTED: OK INPUT=HDMI2 POWER=ON");
            Console.WriteLine("ACTUAL: {response}");
            Console.WriteLine("--------------------------------");

            // 4. Test POWER OFF
            await writer.WriteLineAsync("POWER OFF");
            response = await reader.ReadLineAsync();
            if (response != "OK")
            {
                Console.WriteLine("Test failed: POWER OFF response mismatch");
                return false;
            }
            Console.WriteLine("--------------------------------");
            Console.WriteLine("[PASS]: POWER OFF");
            Console.WriteLine("EXPECTED: OK");
            Console.WriteLine("ACTUAL: {response}");
            Console.WriteLine("--------------------------------");

            // 5. Test STATUS after POWER OFF
            await writer.WriteLineAsync("STATUS");
            response = await reader.ReadLineAsync();
            if (response != "OK INPUT=HDMI2 POWER=OFF")
            {
                Console.WriteLine("Test failed: STATUS response mismatch after POWER OFF");
                return false;
            }
            Console.WriteLine("--------------------------------");
            Console.WriteLine("[PASS]: STATUS after POWER OFF");
            Console.WriteLine("EXPECTED: OK INPUT=HDMI2 POWER=OFF");
            Console.WriteLine("ACTUAL: {response}");
            Console.WriteLine("--------------------------------");

            // 6. Test POWER ON
            await writer.WriteLineAsync("POWER ON");
            response = await reader.ReadLineAsync();
            if (response != "OK")
            {
                Console.WriteLine("Test failed: POWER ON response mismatch");
                return false;
            }
            Console.WriteLine("--------------------------------");
            Console.WriteLine("[PASS]: POWER ON");
            Console.WriteLine("EXPECTED: OK");
            Console.WriteLine("ACTUAL: {response}");
            Console.WriteLine("--------------------------------");

            // 7. Test STATUS after POWER ON
            await writer.WriteLineAsync("STATUS");
            response = await reader.ReadLineAsync();
            if (response != "OK INPUT=HDMI2 POWER=ON")
            {
                Console.WriteLine("Test failed: STATUS response mismatch after POWER ON");
                return false;
            }
            Console.WriteLine("--------------------------------");
            Console.WriteLine("[PASS]: STATUS after POWER ON");
            Console.WriteLine("EXPECTED: OK INPUT=HDMI2 POWER=ON");
            Console.WriteLine("ACTUAL: {response}");
            Console.WriteLine("--------------------------------");

            return true;
        }
        public static async Task ConcurrencyTest(TcpClient client)
        {
            // TODO: later
            return;
        }
    }
}