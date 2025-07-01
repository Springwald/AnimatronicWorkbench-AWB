// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2025 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License

using Awb.Core.Services;
using AwbStudio.Tools;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AwbStudio
{
    internal class AwbDebugWindowLogger(DebugWindow debugWindow) : IAwbLogger, IDisposable
    {
        private DebugWindow? _debugWindow = debugWindow;
        private readonly List<string> _output = [];
        public event EventHandler<string>? OnError;
        public event EventHandler<string>? OnLog;

        public async Task LogErrorAsync(string message)
        {
            OnError?.Invoke(this, message);
            await ShowMsg($"## Error ## {message}");
        }

        public async Task LogAsync(string message)
        {
            OnLog?.Invoke(this, message);
            await ShowMsg(message);
        }

        private async Task ShowMsg(string msg)
        {
            WpfAppInvoker.Invoke(new Action(() =>
            {
                if (_debugWindow?.TextBox != null)
                {
                    _output.Insert(0, msg);
                    while (_output.Count > 80) _output.RemoveAt(_output.Count - 1);
                    _debugWindow.TextBox.Text = string.Join("\r\n", _output);
                }
            }), System.Windows.Threading.DispatcherPriority.Background);
            await Task.CompletedTask;
        }

        public void Close()
        {
            if (_debugWindow != null)
            {
                _debugWindow.Close();
                _debugWindow = null;
            }
        }

        public void Dispose()
        {
            Close();
        }
    }
}
