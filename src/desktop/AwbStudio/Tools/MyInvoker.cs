// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using System;
using System.Windows;
using System.Windows.Threading;

namespace AwbStudio.Tools
{
    internal static class MyInvoker
    {
        public static void Invoke(Action action, DispatcherPriority priority = DispatcherPriority.Background)
        {
            var app = Application.Current;
            if (app == null) return;
            app.Dispatcher.BeginInvoke(priority, action);
        }
    }
}
