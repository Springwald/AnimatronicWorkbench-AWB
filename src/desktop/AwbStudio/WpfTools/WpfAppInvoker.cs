// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using Awb.Core.Tools;
using System;
using System.Windows;
using System.Windows.Threading;

namespace AwbStudio.Tools
{
    public class WpfAppInvoker : IInvoker
    {
        private static Application _app = Application.Current;

        public void Invoke(Action action, bool useBackgroundPriority = true) 
            => Invoke(action, priority: useBackgroundPriority ? DispatcherPriority.Background : DispatcherPriority.Normal);

        public static void Invoke(Action action, DispatcherPriority priority)
        {
            if (_app == null) return;
            _app.Dispatcher.BeginInvoke(priority, action);
        }
    }
}
