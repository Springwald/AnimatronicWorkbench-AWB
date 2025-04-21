// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2025 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License

using Awb.Core.Tools;

namespace AwbStudio.Tools
{
    internal class WpfAppInvokerService : IInvokerService
    {
        private WpfAppInvoker _invoker;

        public WpfAppInvokerService()
        {
            _invoker = new WpfAppInvoker();
        }

        public IInvoker GetInvoker() => _invoker;
    }
}
