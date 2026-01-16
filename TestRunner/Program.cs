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

// TODO: 
// reset state between functional and concurrent tests
// add scripts

using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Persistence;

namespace TestRunner
{
    public class ClientScript
    {
        public string Name { get; }
        public string[] Commands { get; }

        public ClientScript(string name, string[] commands)
        {
            Name = name;
            Commands = commands;
        }
    }
    public class Program
    {
        public static async Task Main(string[] args)
        {
            using (var logService = new TestLogService())
            {
                try
                {
                    using (var client = new TcpClient())
                    {
                        await client.ConnectAsync(IPAddress.Loopback, 13000);
                        bool function_passed = await FunctionalTest(client, logService);

                        if (!function_passed)
                        {
                            await logService.LogTestFailAsync("Functional Tests", "suite", null, null, null, "At least one functional test failed");
                            Console.WriteLine("At least one functional test failed");
                            Environment.Exit(1);
                        }
                        await logService.LogTestPassAsync("Functional Tests", "suite");
                        Console.WriteLine("--------------------------------");
                        Console.WriteLine("All functional tests passed");
                        Console.WriteLine("--------------------------------");
                    }

                    bool concurrency_passed = await ConcurrencyTest(logService);

                    if (!concurrency_passed)
                    {
                        await logService.LogTestFailAsync("Concurrency Tests", "suite", null, null, null, "At least one concurrency test failed");
                        Console.WriteLine("At least one concurrency test failed");
                        Environment.Exit(1);
                    }
                    await logService.LogTestPassAsync("Concurrency Tests", "suite");
                    Console.WriteLine("--------------------------------");
                    Console.WriteLine("All concurrency tests passed");
                    Console.WriteLine("--------------------------------");

                    Environment.Exit(0);
                }
                catch (Exception ex)
                {
                    await logService.LogTestFailAsync("Test Execution", "system", null, null, null, ex.Message);
                    Console.WriteLine($"Test execution failed: {ex.Message}");
                    Environment.Exit(1);
                }
            }
        }

        public static async Task<bool> FunctionalTest(TcpClient client, TestLogService logService)
        {
            NetworkStream stream = client.GetStream();
            var reader = new StreamReader(stream, Encoding.UTF8);
            var writer = new StreamWriter(stream, new UTF8Encoding(false)) { AutoFlush = true };


            // 1. Test STATUS
            await writer.WriteLineAsync("STATUS");

            var readTask = reader.ReadLineAsync();
            if (await Task.WhenAny(readTask, Task.Delay(2000)) != readTask)
            {
                await logService.LogTestFailAsync("STATUS default", "functional", "STATUS", "OK INPUT=HDMI1 POWER=ON", null, "Timeout waiting for response");
                Console.WriteLine("Test 1 failed: Timeout waiting for response");
                return false;
            }
            string? response = readTask.Result;
            if (response == null)
            {
                await logService.LogTestFailAsync("STATUS default", "functional", "STATUS", "OK INPUT=HDMI1 POWER=ON", null, "No response received from device");
                Console.WriteLine("Test 1 failed: No response received from device");
                return false;
            }
            if (response != "OK INPUT=HDMI1 POWER=ON")
            {
                await logService.LogTestFailAsync("STATUS default", "functional", "STATUS", "OK INPUT=HDMI1 POWER=ON", response, "STATUS response mismatch");
                Console.WriteLine("Test 1 failed: STATUS response mismatch");
                Console.WriteLine("EXPECTED: OK INPUT=HDMI1 POWER=ON");
                Console.WriteLine($"ACTUAL: {response}");
                return false;
            }
            await logService.LogTestPassAsync("STATUS default", "functional", "STATUS", "OK INPUT=HDMI1 POWER=ON", response);
            Console.WriteLine("--------------------------------");
            Console.WriteLine("[1/7 PASS]: STATUS default");
            Console.WriteLine("EXPECTED: OK INPUT=HDMI1 POWER=ON");
            Console.WriteLine($"ACTUAL: {response}");
            Console.WriteLine("--------------------------------");

            // 2. Test SET_INPUT HDMI2
            await writer.WriteLineAsync("SET_INPUT HDMI2");

            readTask = reader.ReadLineAsync();
            if (await Task.WhenAny(readTask, Task.Delay(2000)) != readTask)
            {
                Console.WriteLine("Test 2 failed: Timeout waiting for response");
                return false;
            }
            response = readTask.Result;
            if (response == null)
            {
                Console.WriteLine("Test 2 failed: No response received from device");
                return false;
            }
            if (response != "OK")
            {
                Console.WriteLine("Test 2 failed: SET_INPUT response mismatch");
                Console.WriteLine("EXPECTED: OK");
                Console.WriteLine($"ACTUAL: {response}");
                return false;
            }
            Console.WriteLine("--------------------------------");
            Console.WriteLine("[2/7 PASS]: SET_INPUT HDMI2");
            Console.WriteLine("EXPECTED: OK");
            Console.WriteLine($"ACTUAL: {response}");
            Console.WriteLine("--------------------------------");
            // 3. Test STATUS after SET_INPUT
            await writer.WriteLineAsync("STATUS");
            readTask = reader.ReadLineAsync();
            if (await Task.WhenAny(readTask, Task.Delay(2000)) != readTask)
            {
                Console.WriteLine("Test 3 failed: Timeout waiting for response");
                return false;
            }
            response = readTask.Result;
            if (response == null)
            {
                Console.WriteLine("Test 3 failed: No response received from device");
                return false;
            }
            if (response != "OK INPUT=HDMI2 POWER=ON")
            {
                Console.WriteLine("Test 3 failed: STATUS response mismatch after SET_INPUT");
                Console.WriteLine("EXPECTED: OK INPUT=HDMI2 POWER=ON");
                Console.WriteLine($"ACTUAL: {response}");
                return false;
            }
            Console.WriteLine("--------------------------------");
            Console.WriteLine("[3/7 PASS]: STATUS after SET_INPUT");
            Console.WriteLine("EXPECTED: OK INPUT=HDMI2 POWER=ON");
            Console.WriteLine($"ACTUAL: {response}");
            Console.WriteLine("--------------------------------");

            // 4. Test POWER OFF
            await writer.WriteLineAsync("POWER OFF");
            readTask = reader.ReadLineAsync();
            if (await Task.WhenAny(readTask, Task.Delay(2000)) != readTask)
            {
                Console.WriteLine("Test 4 failed: Timeout waiting for response");
                return false;
            }
            response = readTask.Result;
            if (response == null)
            {
                Console.WriteLine("Test 4 failed: No response received from device");
                return false;
            }
            if (response != "OK")
            {
                Console.WriteLine("Test 4 failed: POWER OFF response mismatch");
                Console.WriteLine("EXPECTED: OK");
                Console.WriteLine($"ACTUAL: {response}");
                return false;
            }
            Console.WriteLine("--------------------------------");
            Console.WriteLine("[4/7 PASS]: POWER OFF");
            Console.WriteLine("EXPECTED: OK");
            Console.WriteLine($"ACTUAL: {response}");
            Console.WriteLine("--------------------------------");

            // 5. Test STATUS after POWER OFF
            await writer.WriteLineAsync("STATUS");
            readTask = reader.ReadLineAsync();
            if (await Task.WhenAny(readTask, Task.Delay(2000)) != readTask)
            {
                Console.WriteLine("Test 5 failed: Timeout waiting for response");
                return false;
            }
            response = readTask.Result;
            if (response == null)
            {
                Console.WriteLine("Test 5 failed: No response received from device");
                return false;
            }
            if (response != "OK INPUT=HDMI2 POWER=OFF")
            {
                Console.WriteLine("Test 5 failed: STATUS response mismatch after POWER OFF");
                Console.WriteLine("EXPECTED: OK INPUT=HDMI2 POWER=OFF");
                Console.WriteLine($"ACTUAL: {response}");
                return false;
            }
            Console.WriteLine("--------------------------------");
            Console.WriteLine("[5/7 PASS]: STATUS after POWER OFF");
            Console.WriteLine("EXPECTED: OK INPUT=HDMI2 POWER=OFF");
            Console.WriteLine($"ACTUAL: {response}");
            Console.WriteLine("--------------------------------");

            // 6. Test POWER ON
            await writer.WriteLineAsync("POWER ON");
            readTask = reader.ReadLineAsync();
            if (await Task.WhenAny(readTask, Task.Delay(2000)) != readTask)
            {
                Console.WriteLine("Test 6 failed: Timeout waiting for response");
                return false;
            }
            response = readTask.Result;
            if (response == null)
            {
                Console.WriteLine("Test 6 failed: No response received from device");
                return false;
            }
            if (response != "OK")
            {
                Console.WriteLine("Test 6 failed: POWER ON response mismatch");
                Console.WriteLine("EXPECTED: OK");
                Console.WriteLine($"ACTUAL: {response}");
                return false;
            }
            Console.WriteLine("--------------------------------");
            Console.WriteLine("[6/7 PASS]: POWER ON");
            Console.WriteLine("EXPECTED: OK");
            Console.WriteLine($"ACTUAL: {response}");
            Console.WriteLine("--------------------------------");

            // 7. Test STATUS after POWER ON
            await writer.WriteLineAsync("STATUS");
            readTask = reader.ReadLineAsync();
            if (await Task.WhenAny(readTask, Task.Delay(2000)) != readTask)
            {
                Console.WriteLine("Test 7 failed: Timeout waiting for response");
                return false;
            }
            response = readTask.Result;
            if (response == null)
            {
                Console.WriteLine("Test 7 failed: No response received from device");
                return false;
            }
            if (response != "OK INPUT=HDMI2 POWER=ON")
            {
                Console.WriteLine("Test 7 failed: STATUS response mismatch after POWER ON");
                Console.WriteLine("EXPECTED: OK INPUT=HDMI2 POWER=ON");
                Console.WriteLine($"ACTUAL: {response}");
                return false;
            }
            Console.WriteLine("--------------------------------");
            Console.WriteLine("[7/7 PASS]: STATUS after POWER ON");
            Console.WriteLine("EXPECTED: OK INPUT=HDMI2 POWER=ON");
            Console.WriteLine($"ACTUAL: {response}");
            Console.WriteLine("--------------------------------");

            // 8. Test SET_INPUT HDMI1
            await writer.WriteLineAsync("SET_INPUT HDMI1");
            readTask = reader.ReadLineAsync();
            if (await Task.WhenAny(readTask, Task.Delay(2000)) != readTask)
            {
                Console.WriteLine("Test 8 failed: Timeout waiting for response");
                return false;
            }
            response = readTask.Result;
            if (response == null)
            {
                Console.WriteLine("Test 8 failed: No response received from device");
                return false;
            }
            if (response != "OK")
            {
                Console.WriteLine("Test 8 failed: SET_INPUT response mismatch");
                Console.WriteLine("EXPECTED: OK");
                Console.WriteLine($"ACTUAL: {response}");
                return false;
            }
            Console.WriteLine("--------------------------------");
            Console.WriteLine("[8/8 PASS]: SET_INPUT HDMI1");
            Console.WriteLine("EXPECTED: OK");
            Console.WriteLine($"ACTUAL: {response}");
            Console.WriteLine("--------------------------------");

            return true;
        }
        // concurrency test doesn,t take in a tcp client because it will create multiple clients
        // concurrencytest is like an orchestrator, we'll have multiple clientscenarios and send N clients to each one 
        public static async Task<bool> ConcurrencyTest(TestLogService logService)
        {
            // 1. Basic concurrency test
            // 2. State contention test
            // 3. Ordering robustness
            // 4. Soak test
            // 5. Error handling test
            var readOnlyScript = new ClientScript(
                name: "ReadOnly",
                commands: new string[] {
                    "STATUS",
                    "STATUS",
                }
            );

            var mutatingScript = new ClientScript(
                name: "Mutating",
                commands: new string[] {
                    "STATUS",
                    "SET_INPUT HDMI2",
                    "STATUS",
                    "POWER OFF",
                    "STATUS",
                    "POWER ON",
                    "SET_INPUT HDMI1",
                    "STATUS",
                }
            );

            var contentionScript = new ClientScript(
                name: "Contention",
                commands: new string[] {
                    "SET_INPUT HDMI2",
                    "STATUS",
                    "POWER OFF",
                    "STATUS",
                    "POWER ON",
                    "STATUS",
                }
            );

            var orderingScript = new ClientScript(
                name: "Ordering",
                commands: new string[] {
                    "POWER OFF",
                    "STATUS",
                    "POWER ON",
                    "STATUS",
                    "SET_INPUT HDMI2",
                    "STATUS",
                    "SET_INPUT HDMI1",
                    "STATUS",
                }
            );

            var soakScript = new ClientScript(
                name: "Soak",
                commands: new string[] {
                    "STATUS",
                    "SET_INPUT HDMI1",
                    "STATUS",
                    "SET_INPUT HDMI2",
                    "STATUS",
                    "POWER OFF",
                    "STATUS",
                    "POWER ON",
                    "STATUS",
                }
            );

            var errorHandlingScript = new ClientScript(
                name: "ErrorHandling",
                commands: new string[] {
                    "BADCOMMAND",
                    "STATUS",
                    "POWER MAYBE",
                    "STATUS",
                    "SET_INPUT HDMI9",
                    "STATUS",
                }
            );

            var resetStateScript = new ClientScript(
                name: "ResetState",
                commands: new string[] {
                    "POWER ON",
                    "SET_INPUT HDMI1",
                }
            );

            if (!await RunConcurrencyTest("ReadOnly", new[] { (readOnlyScript, 100) }, logService))
            {
                return false;
            }
            if (!await RunConcurrencyTest("Mutating", new[] { (mutatingScript, 5) }, logService))
            {
                return false;
            }
            if (!await RunConcurrencyTest("Contention", new[] { (readOnlyScript, 5), (contentionScript, 5) }, logService))
            {
                return false;
            }

            if (!await RunConcurrencyTest("Ordering", new[] { (readOnlyScript, 5), (orderingScript, 5) }, logService))
            {
                return false;
            }

            if (!await RunConcurrencyTest("Soak", new[] { (readOnlyScript, 5), (soakScript, 5) }, logService))
            {
                return false;
            }

            if (!await RunConcurrencyTest("ErrorHandling", new[] { (readOnlyScript, 5), (errorHandlingScript, 5) }, logService))
            {
                return false;
            }

            await RunConcurrencyTest("ResetState", new[] { (resetStateScript, 1) }, logService);
            return true;
        }
        private static async Task<bool> RunConcurrencyTest(string testName, (ClientScript script, int count)[] groups, TestLogService logService)
        {
            Console.WriteLine($"Running {testName}");

            var tasks = new List<Task<bool>>();
            int clientId = 0;

            foreach (var (script, count) in groups)
            {
                for (int i = 0; i < count; i++)
                {
                    int id = clientId++;
                    tasks.Add(Task.Run(() => ClientRunner(id, script)));
                }
            }

            bool[] results = await Task.WhenAll(tasks);

            if (results.Any(r => !r))
            {
                await logService.LogTestFailAsync(testName, "concurrency", null, null, null, "One or more client tests failed");
                Console.WriteLine($"[FAIL] {testName}");
                return false;
            }

            await logService.LogTestPassAsync(testName, "concurrency");
            Console.WriteLine($"[PASS] {testName}");
            return true;
        }

        private static bool IsValidStatusResponse(string response)
        {
            if (!response.StartsWith("OK"))
            {
                return false;
            }
            // expected format: OK INPUT=HDMI1 POWER=ON
            var parts = response.Split(' ');

            if (parts.Length < 3)
            {
                return false;
            }

            var inputPart = parts.FirstOrDefault(p => p.StartsWith("INPUT="));
            var powerPart = parts.FirstOrDefault(p => p.StartsWith("POWER="));

            if (inputPart == null || powerPart == null)
            {
                return false;
            }

            var input = inputPart.Split('=')[1];
            var power = powerPart.Split('=')[1];

            if (input != "HDMI1" && input != "HDMI2" && input != "HDMI3" && input != "HDMI4")
            {
                return false;
            }

            if (power != "ON" && power != "OFF")
            {
                return false;
            }
            return true;
        }
        private static async Task<bool> ClientRunner(int clientId, ClientScript script)
        {
            try
            {
                using var client = new TcpClient();
                await client.ConnectAsync(IPAddress.Loopback, 13000);

                using var stream = client.GetStream();
                using var reader = new StreamReader(stream, Encoding.UTF8);
                using var writer = new StreamWriter(stream, new UTF8Encoding(false))
                {
                    AutoFlush = true
                };

                foreach (var command in script.Commands)
                {
                    await writer.WriteLineAsync(command);
                    var readTask = reader.ReadLineAsync();
                    if (await Task.WhenAny(readTask, Task.Delay(2000)) != readTask)
                    {
                        Console.WriteLine(
                            $"Client {clientId} ({script.Name}) failed: Timeout waiting for response"
                        );
                        return false;
                    }
                    string? response = readTask.Result;

                    if (response == null)
                    {
                        Console.WriteLine(
                            $"Client {clientId} ({script.Name}) failed: No response"
                        );
                        return false;
                    }

                    // ERROR cases FIRST
                    if (command.StartsWith("BAD") ||
                        command.StartsWith("POWER MAYBE") ||
                        command.StartsWith("SET_INPUT HDMI9"))
                    {

                        if (!response.StartsWith("ERROR"))
                        {
                            Console.WriteLine(
                                $"Client {clientId} ({script.Name}) expected ERROR for '{command}', got '{response}'"
                            );
                            return false;
                        }

                        continue;
                    }

                    // STATUS invariant
                    if (command == "STATUS")
                    {
                        if (!IsValidStatusResponse(response))
                        {
                            Console.WriteLine(
                                $"Client {clientId} ({script.Name}) invalid STATUS: {response}"
                            );
                            return false;
                        }
                        continue;
                    }

                    // Valid mutating commands
                    if (command.StartsWith("SET_INPUT"))
                    {
                        if (response != "OK")
                        {
                            Console.WriteLine(
                                $"Client {clientId} ({script.Name}) SET_INPUT failed: {response}"
                            );
                            return false;
                        }
                        continue;
                    }

                    if (command.StartsWith("POWER"))
                    {
                        if (response != "OK")
                        {
                            Console.WriteLine(
                                $"Client {clientId} ({script.Name}) POWER failed: {response}"
                            );
                            return false;
                        }
                        continue;
                    }

                    // Unknown command 
                    Console.WriteLine(
                        $"Client {clientId} ({script.Name}) unhandled command '{command}'"
                    );
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"Client {clientId} ({script.Name}) failed: {ex.Message}"
                );
                return false;
            }
        }
    }
}