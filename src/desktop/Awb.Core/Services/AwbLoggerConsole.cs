// Animatronic WorkBench core routines
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
        Task LogAsync(string message);
        Task LogErrorAsync(string message);
        event EventHandler<string>? OnError;
        event EventHandler<string>? OnLog;
    }

    public class AwbLoggerConsole : IAwbLogger
    {
        private bool _throwWhenInDebugMode;

        public event EventHandler<string>? OnError;
        public event EventHandler<string>? OnLog;

        public AwbLoggerConsole(bool throwWhenInDebugMode)
        {
            _throwWhenInDebugMode = throwWhenInDebugMode;
        }

        public async Task LogErrorAsync(string message)
        {
            OnError?.Invoke(this, message);
            if (_throwWhenInDebugMode && Debugger.IsAttached) throw new Exception(message);
            Console.BackgroundColor = ConsoleColor.Red;
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("Error:");
            SetStandardColor();
            Console.WriteLine($" {message}");
            await Task.CompletedTask;
        }

        public async Task LogAsync(string message)
        {
            OnLog?.Invoke(this, message);
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
