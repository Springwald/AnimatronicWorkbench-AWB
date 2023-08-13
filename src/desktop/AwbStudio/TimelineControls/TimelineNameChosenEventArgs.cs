// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2023 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

namespace AwbStudio.TimelineControls
{
    public class TimelineNameChosenEventArgs
    {
        public string FileName { get; }

        public TimelineNameChosenEventArgs(string filename)
        {
            FileName = filename;
        }
    }
}
