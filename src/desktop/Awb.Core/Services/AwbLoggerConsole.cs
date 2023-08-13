// AnimatronicWorkBench core routines
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2023 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using System.Diagnostics;

namespace Awb.Core.Services
{
    public interface IAwbLogger
    {
        Task Log(string message);
        Task LogError(string message);
    }

    public class AwbLoggerConsole : IAwbLogger
    {
        private bool _throwWhenInDebugMode;

        public AwbLoggerConsole(bool throwWhenInDebugMode)
        {
            _throwWhenInDebugMode = throwWhenInDebugMode;
        }

        public async Task LogError(string message)
        {
            if (_throwWhenInDebugMode && Debugger.IsAttached) throw new Exception(message);
            Console.BackgroundColor = ConsoleColor.Red;
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("Error:");
            SetStandardColor();
            Console.WriteLine($" {message}");
            await Task.CompletedTask;
        }

        public async Task Log(string message)
        {
            SetStandardColor();
            Console.WriteLine(message);
            await Task.CompletedTask;
        }

        private static void SetStandardColor()
        {
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.White;
        }
    }
}
