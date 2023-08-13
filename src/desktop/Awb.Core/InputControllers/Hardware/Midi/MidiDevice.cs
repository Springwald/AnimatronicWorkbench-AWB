// Animatronic WorkBench core routines
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2023 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License


// Based on code from Project MIDI keyboard
// https://github.com/millennIumAMbiguity/MIDI-Keyboard
// (C) millennIumAMbiguity - Licensed under MIT 

using Awb.Core.Services;

namespace Awb.Core.InputControllers.Midi
{
    public class MidiDevice : IDisposable
    {
        protected readonly IAwbLogger _awbLogger;
        protected MidiPort? _midiPort;

        public bool Available = false;

        public MidiDevice(string deviceName, IAwbLogger awbLogger)
        {
            _awbLogger = awbLogger;
            if (OpenChannels(deviceName) == false)
            {
                Available = false;
                return;
            }
            Available = true;
        }

        private bool OpenChannels(string deviceName)
        {
            var foundNames = new List<string>();

            // Find INPUT channel by devicename
            int? inChannel = null;
            _midiPort = new MidiPort();
            for (int channel = 0; channel < _midiPort.InputCount(); channel++)
            {
                var foundDeviceName = NativeMethods.midiInGetDevCaps((IntPtr)channel);
                foundNames.Add(foundDeviceName);
                if (foundDeviceName?.Equals(deviceName) == true)
                {
                    inChannel = channel;
                    _midiPort.Open(channel);
                    break;
                }
            }
            if (inChannel != null)
            {
                _awbLogger.Log($"Midi: Found '{deviceName}' on INPUT channel {inChannel}");
            }
            else
            {
                _awbLogger.LogError($"Midi: INPUT Device'{deviceName}' not found. Found {string.Join(", ", foundNames.Select(n => "'" + n + "'"))} instead");
                _midiPort?.Close();
                _midiPort?.Dispose();
                _midiPort = null;
                return false;
            }

            // find OUTPUT channel by devicename
            int? outChannel = null;
            for (int channel = 0; channel < _midiPort.OutputCount(); channel++)
            {
                if (deviceName == NativeMethods.midiOutGetDevCaps((IntPtr)channel))
                {
                    outChannel = channel;
                    _midiPort.OpenOut(channel);
                    break;
                }
            }

            if (inChannel != null)
            {
                _awbLogger.Log($"Midi: Found '{deviceName}' on OUTPUT channel {outChannel}");
            }
            else
            {
                _awbLogger.LogError($"Midi: OUTPUT Device'{deviceName}' not found. Found {string.Join(", ", foundNames.Select(n => "'" + n + "'"))} instead");
                _midiPort?.CloseOut();
                _midiPort?.Close();
                _midiPort?.Dispose();
                _midiPort = null;
                return false;
            }

            _midiPort.Start();
            return true;

        }

        public void Dispose()
        {
            _midiPort?.Stop();
            _midiPort?.Close();
            _midiPort?.CloseOut();
            _midiPort?.Dispose();
            _midiPort = null;
        }
    }
}
