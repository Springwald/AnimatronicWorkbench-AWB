// Animatronic WorkBench core routines
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

namespace Awb.Core.Export.ExporterParts.CustomCode
{
    internal class CustomCodeRegionContent
    {
        private List<Region> _regions { get; init; } = new List<Region>();
        public Region[] Regions => _regions.ToArray();

        public void AddRegion(string name, string content)
        {
            _regions.Add(new Region { Name = name, Content = content });
        }

        public record Region
        {
            public required string Name { get; init; }
            public required string Content { get; init; }
        }

    }
}
