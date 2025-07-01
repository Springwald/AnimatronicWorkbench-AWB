// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2025 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License

using Awb.Core.Tools;

namespace MidiControllerLiveControl
{
    internal class MockInvoker : IInvoker
    {
        public void Invoke(Action action, bool useBackgroundPriority = true)
        {
            action();
        }
    }
}
