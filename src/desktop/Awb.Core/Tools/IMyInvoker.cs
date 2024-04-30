// Animatronic WorkBench core routines
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

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
