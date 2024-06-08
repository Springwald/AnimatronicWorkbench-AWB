// Animatronic WorkBench core routines
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using System.Text.Json.Serialization;

namespace Awb.Core.Project.Various
{
    public class InputConfig : IProjectObjectListable
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public int? IoPin { get; set; }

        [JsonIgnore]
        public string TitleShort => Title ?? $"no title for input '{Id}'";

        [JsonIgnore]
        public string TitleDetailled => $"Input {TitleShort}";

        public InputConfig(int id, string title)
        {
            Id = id;
            Title = title;
        }
    }
}
