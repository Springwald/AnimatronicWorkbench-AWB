// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2025 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License

namespace Awb.Core.Tools
{
    public interface IInvoker
    {
        void Invoke(Action action, bool useBackgroundPriority = true);
    }

    public interface IInvokerService
    {
        IInvoker GetInvoker();
    }
}
