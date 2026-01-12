/*
This file will:
- Decide what commands exist and what responses look like i.e:
STATUS
-> OK INPUT=HDMI1 POWER=ON

SET_INPUT HDMI2
-> OK

POWER OFF
-> OK

Commands will:
- read from state
- mutate state
- return responses

We do this to fake our hardware and make it appear as if we are a real device
*/

namespace DeviceSimulator.State {
    class DeviceState {
        // simulates hardware memory
        public bool PowerOn = true;
        public string Input = "HDMI1"; 
    }
}