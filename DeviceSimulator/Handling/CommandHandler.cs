/*
This is where STATUS, SET_INPUT, and POWER ON/OFF commands are implemented
string command + devicestate -> string response
It will parse commands, validate arguments, mutate devicestate, and return explicit OK/ERROR responses

STATUS -> return curr state
SET_INPUT <value> -> update input
POWER ON -> power on
POWER OFF -> power off

invalid commands -> ERROR <reason>
*/
using System;
using DeviceSimulator.State;
namespace DeviceSimulator.Handling {

    class CommandHandler {
        private readonly DeviceState _state;
        public CommandHandler(DeviceState state) {
            _state = state;
        }

        public string HandleCommand(string command) {
        // normalize input, split command tokens, route to specific handlers, and return responses
        try {
            string[] tokens = command.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            // tokens[0] is the command, tokens[1] is the argument
            if (tokens.Length == 0) {
                return Error("Empty command");
            }
            string commandName = tokens[0].ToUpper();
            switch (commandName) {
                case "STATUS":
                    return HandleStatus();
                case "SET_INPUT":
                    if (tokens.Length != 2) {
                        return Error("Invalid number of arguments");
                    }
                    return HandleSetInput(tokens[1]);
                case "POWER":
                    if (tokens.Length != 2) {
                        return Error("Invalid number of arguments");
                    }
                    return HandlePower(tokens[1]);
                default:
                    return Error($"Unknown command: {commandName}");
            }
        } catch (Exception ex) {
            return $"ERROR: {ex.Message}";
        }
    }

        private string HandleStatus() {
            // read devicestate and format response
            return $"OK INPUT={_state.Input} POWER={(_state.PowerOn ? "ON" : "OFF")}";
        }

        private string HandleSetInput(string input) {
            // validate input, mutate devicestate, return ok or err
            string normalized = input.ToUpper();

            if (normalized != "HDMI1" && normalized != "HDMI2" && normalized != "HDMI3" && normalized != "HDMI4") {
                return Error($"Invalid input value: {input}");
            }

            _state.Input = normalized;
            return "OK";
        }

        private string HandlePower(string value) {
            // validate on/off, mutate devicestate, return ok or err
            string normalized = value.ToUpper();

            if (normalized != "ON" && normalized != "OFF") {
                return Error($"Invalid power value: {value}");
            }

            if (normalized == "ON") {
                _state.PowerOn = true;
            } else {
                _state.PowerOn = false;
            }
            return "OK";
        }

        private string Error(string message) {
            return $"ERROR: {message}";
        }
    }
}