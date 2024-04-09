// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

namespace AwbStudio.FileManagement
{
    public class TimelineMetaData
    {
        public string Id { get; set; }
        public string Title { get; }
        public int StateId { get; }
        public string StateName { get; }

        public TimelineMetaData(string id, string title, int stateId, string stateName)
        {
            Id = id;
            Title = title;
            StateId = stateId;
            StateName = stateName;
        }
    }
}
