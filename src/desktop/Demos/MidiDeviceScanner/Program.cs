// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2025 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License

// Based on code from Project MIDI keyboard
// https://github.com/millennIumAMbiguity/MIDI-Keyboard
// (C) millennIumAMbiguity - Licensed under MIT 

using Awb.Core.InputControllers.Midi;

internal class Program
{
    private static void Main(string[] args)
    {
        var midi = new MidiPort();

        Console.WriteLine("what midi port do you whant to use");
        {
            Console.ForegroundColor = ConsoleColor.Black;
            for (int i = 0; i < midi.InputCount(); i++)
            {
                if (i % 2 == 0)
                    Console.BackgroundColor = ConsoleColor.Gray;
                else
                    Console.BackgroundColor = ConsoleColor.White;

                Console.WriteLine(i + ".\t" + NativeMethods.midiInGetDevCaps((IntPtr)i).PadRight(32, ' '));
            }

            Console.ResetColor();
        }

        int inChanel;
        Console.Write("port: ");
        int.TryParse(Console.ReadLine(), out int resultat2);
        Console.WriteLine("value set to " + resultat2);
        Console.WriteLine("press ESC to exit");
        inChanel = resultat2;

        int? outChanel = null;
        var deviceNameInput = NativeMethods.midiInGetDevCaps((IntPtr)inChanel);
        for (int i = 0; i < midi.OutputCount(); i++)
        {
            if (deviceNameInput == NativeMethods.midiOutGetDevCaps((IntPtr)i))
            {
                outChanel = i;
                break;
            }
        }

        if (outChanel == null)
        {
            Console.WriteLine("No output device found");
            return;
        }

        var ok = midi.Open(inChanel)
            && midi.OpenOut(outChanel.Value)
            && midi.Start();

        Console.WriteLine("Open midi " + ok);


        //for (int knob = 0; knob <= 8; knob++)
        //{
        //    for (int pos = 0; pos < 255; pos += 1)
        //    {
        //        midi.MidiOutMsg(0xba, (byte)knob, (byte)pos);  // set knob position
        //    }
        //}

        int old = 0;

        while (!Console.KeyAvailable)
        {
            int value = midi._p;
            if (old != value)
            {
                string valueHex = midi._pS;
                string hex4 = valueHex[^4..];

                if (hex4.Substring(hex4.Length - 2) == "D0")
                {
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
                    /*
                    if (valueHex.Length > 4)
                    {

                    }*/

                    Console.Write(valueHex.PadLeft(6, ' ').Substring(0, 2));
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write(hex4 + " ");
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine(value);
                    Console.ForegroundColor = ConsoleColor.White;
                }
                old = value;
            }
        }

        midi.Stop();
        midi.Close();
        midi.CloseOut();
        midi.Dispose();
    }
}