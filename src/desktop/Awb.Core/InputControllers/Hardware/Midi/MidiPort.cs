// Animatronic WorkBench core routines
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2023 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

// Based on code from Project MIDI keyboard
// https://github.com/millennIumAMbiguity/MIDI-Keyboard
// (C) millennIumAMbiguity - Licensed under MIT 

using System.Runtime.InteropServices;

namespace Awb.Core.InputControllers.Midi
{

    public class MidiPort : IDisposable
    {
        public static readonly EventWaitHandle _waitHandle = new AutoResetEvent(false);
        private readonly NativeMethods.MidiInProc _midiInProc;
        private readonly NativeMethods.MidiOutProc _midiOutProc;
        private IntPtr _handle, _handleOut;
        private bool _isOpenOut, _isOpenIn, _isStarted;
        public EventHandler<MidiInputEventArgs> OnInputEvent;

        public int _p;
        public string _pS = "";

        public MidiPort()
        {
            _midiInProc = MidiProc;
            _handle = IntPtr.Zero;
            _midiOutProc = MidiProc;
            _handleOut = IntPtr.Zero;
        }


        public bool SendMidiMessage(params byte[] data)
        {
            byte[] bytes = new byte[4]
            {
                data[0],
                data[1],
                data[2],
                0,
            };

            //hmidi is an IntPtr obtained via midiOutOpen or other means.
            int msg = BitConverter.ToInt32(bytes, 0);

            return NativeMethods.midiOutShortMsg(_handleOut, msg)
                == NativeMethods.MMSYSERR_NOERROR;
        }

        public int InputCount() => NativeMethods.midiInGetNumDevs();

        public bool Close()
        {
            _isOpenIn = false;
            bool result = NativeMethods.midiInClose(_handle)
                       == NativeMethods.MMSYSERR_NOERROR;
            _handle = IntPtr.Zero;

            result = result && NativeMethods.midiOutClose(_handleOut)
                       == NativeMethods.MMSYSERR_NOERROR;
            _handleOut = IntPtr.Zero;

            return result;
        }

        public bool Open(int id)
        {
            _isOpenIn = true;
            return NativeMethods.midiInOpen(
                       out _handle,
                       id,
                       _midiInProc,
                       IntPtr.Zero,
                       NativeMethods.CALLBACK_FUNCTION)
                == NativeMethods.MMSYSERR_NOERROR;
        }

        public bool Start()
        {
            _isStarted = true;
            return NativeMethods.midiInStart(_handle)
                == NativeMethods.MMSYSERR_NOERROR;
        }

        public bool Stop()
        {
            _isStarted = false;
            return NativeMethods.midiInStop(_handle)
                == NativeMethods.MMSYSERR_NOERROR;
        }

        private void MidiProc(
            IntPtr hMidiIn,
            int wMsg,
            IntPtr dwInstance,
            int dwParam1,
            int dwParam2)
        {
            _pS = dwParam1.ToString("X").PadLeft(4, '0'); // Gives you hexadecimal
            _p = dwParam1;

            _waitHandle.Set();

            // Receive messages here

            var value = _p;
            string valueHex = _pS;
            string hex4 = valueHex.Substring(valueHex.Length - 4);

            if (hex4.Substring(hex4.Length - 2) == "D0")
            {
                // what is this?
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write(valueHex.PadLeft(6, ' ').Substring(0, 4));
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.Write("D0 ");
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine(value);
                Console.ForegroundColor = ConsoleColor.White;
            }
            else
            {
                var command = hex4;
                var commandValue = valueHex.PadLeft(6, ' ').Substring(0, 2);
                if (OnInputEvent != null)
                {
                    OnInputEvent(this, new MidiInputEventArgs(command, commandValue));
                }
            }
        }


        public bool MidiOutMsg(byte cmd, byte pitch, byte velocity)
        {
            //hmidi is an IntPtr obtained via midiOutOpen or other means.
            byte[] data = new byte[4];
            data[0] = cmd;     //note on, channel 0
            data[1] = pitch;    //pitch
            data[2] = velocity; //velocity
            int msg = BitConverter.ToInt32(data, 0);

            return NativeMethods.midiOutShortMsg(_handleOut, msg)
                == NativeMethods.MMSYSERR_NOERROR;
        }

        public int OutputCount() => NativeMethods.midiOutGetNumDevs();

        public bool CloseOut()
        {
            _isOpenOut = false;
            bool result = NativeMethods.midiOutClose(_handleOut)
                       == NativeMethods.MMSYSERR_NOERROR;
            _handleOut = IntPtr.Zero;
            return result;
        }

        public bool OpenOut(int id)
        {
            _isOpenOut = true;
            return NativeMethods.midiOutOpen(
                       out _handleOut,
                       id,
                       _midiOutProc,
                       IntPtr.Zero,
                       NativeMethods.CALLBACK_FUNCTION)
                == NativeMethods.MMSYSERR_NOERROR;
        }

        public bool MidiOutReset() =>
            NativeMethods.MidiOutReset(_handleOut)
         == NativeMethods.MMSYSERR_NOERROR;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing) return;
            if (_isStarted) Stop();
            if (_isOpenIn) Close();
            if (_isOpenOut) CloseOut();
        }
    }

#pragma warning disable IDE1006 // Naming Styles
    public static class NativeMethods
    {
        internal const int MMSYSERR_NOERROR = 0;
        internal const int CALLBACK_FUNCTION = 0x00030000;

        public static string midiInGetDevCaps(IntPtr uDeviceID)
        {
            midiInGetDevCaps(
                uDeviceID, out MIDIINCAPS caps,
                (uint)Marshal.SizeOf(typeof(MIDIINCAPS)));
            return caps.szPname;
        }

        public static string midiOutGetDevCaps(IntPtr uDeviceID)
        {
            midiOutGetDevCaps(
                uDeviceID, out MIDIINCAPS caps,
                (uint)Marshal.SizeOf(typeof(MIDIINCAPS)));
            return caps.szPname;
        }

        [DllImport("winmm.dll")] internal static extern int midiInGetNumDevs();

        [DllImport("winmm.dll", SetLastError = true)]
        internal static extern int midiInGetDevCaps(
            IntPtr uDeviceID, out MIDIINCAPS caps,
            uint cbMidiInCaps);

        [DllImport("winmm.dll", SetLastError = true)]
        internal static extern int midiOutGetDevCaps(
            IntPtr uDeviceID, out MIDIINCAPS caps,
            uint cbMidiInCaps);

        [DllImport("winmm.dll")]
        internal static extern int midiInClose(
            IntPtr hMidiIn);

        [DllImport("winmm.dll")]
        internal static extern int midiInOpen(
            out IntPtr lphMidiIn,
            int uDeviceID,
            MidiInProc dwCallback,
            IntPtr dwCallbackInstance,
            int dwFlags);

        [DllImport("winmm.dll")]
        internal static extern int midiInStart(
            IntPtr hMidiIn);

        [DllImport("winmm.dll")]
        internal static extern int midiInStop(
            IntPtr hMidiIn);

        [DllImport("winmm.dll")]
        internal static extern int midiOutShortMsg(
            IntPtr hMidiOut,
            int dwMsg
        );

        [DllImport("winmm.dll")] internal static extern int midiOutGetNumDevs();

        [DllImport("winmm.dll")]
        internal static extern int midiOutOpen(
            out IntPtr lphMidiIn,
            int uDeviceID,
            MidiOutProc dwCallback,
            IntPtr dwCallbackInstance,
            int dwFlags);


        [DllImport("winmm.dll")]
        internal static extern int midiOutClose(
            IntPtr hMidiIn);

        [DllImport("winmm.dll")]
        internal static extern int MidiOutReset(
            IntPtr hMidiIn);

        [StructLayout(LayoutKind.Sequential)]
        [Serializable]
        public struct MIDIINCAPS
        {
            public ushort wMid;
            public ushort wPid;
            public uint vDriverVersion;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string szPname;

            public uint dwSupport;
        }

        internal delegate void MidiInProc(
            IntPtr hMidiIn,
            int wMsg,
            IntPtr dwInstance,
            int dwParam1,
            int dwParam2);


        internal delegate void MidiOutProc(
            IntPtr hMidiIn,
            int wMsg,
            IntPtr dwInstance,
            int dwParam1,
            int dwParam2);
    }
}
