// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2023 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

namespace AwbStudio.FileManagement
{
    public class TimelineMetaData
    {
        public string Title { get; }
        public int StateId { get; }
        public string StateName { get; }

        public TimelineMetaData(string title, int stateId, string stateName)
        {
            Title = title;
            StateId = stateId;
            StateName = stateName;
        }
    }
}
