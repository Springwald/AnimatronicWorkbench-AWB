﻿// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2023 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using Awb.Core.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AwbStudio
{
    internal class AwbDebugWindowLogger : IAwbLogger, IDisposable
    {
        private DebugWindow? _debugWindow;
        private List<string> _output = new List<string>();

        public AwbDebugWindowLogger(DebugWindow debugWindow)
        {
            _debugWindow = debugWindow;
        }

        public async Task LogError(string message) => await ShowMsg($"## Error ## {message}");

        public async Task Log(string message) => await ShowMsg(message);

        private async Task ShowMsg(string msg)
        {
            MyInvoker.Invoke(new Action(() =>
            {
                if (_debugWindow?.TextBox != null)
                {
                    _output.Insert(0, msg);
                    while (_output.Count > 80) _output.RemoveAt(_output.Count - 1);
                    _debugWindow.TextBox.Text = string.Join("\r\n", _output);
                }
            }));
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
