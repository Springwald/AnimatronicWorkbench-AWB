// Animatronic WorkBench core routines
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

namespace Awb.Core.Project
{
    public class InputConfig
    {
        public int Id { get;  set; }
        public string Title { get;  set; }
        public int? IoPin { get;  set; } 

        public InputConfig(int id, string title)
        {
            Id = id;
            Title = title;
        }
    }
}
