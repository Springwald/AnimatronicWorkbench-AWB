// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using Awb.Core.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
