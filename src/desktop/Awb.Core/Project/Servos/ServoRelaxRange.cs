// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2025 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License

namespace Awb.Core.Project.Servos
{
    public class ServoRelaxRange
    {
        public int MinValue { get; set; }
        public int MaxValue { get; set; }

        public static ServoRelaxRange[] FromString(string? relaxRangesAsString)
        {
            if (string.IsNullOrWhiteSpace(relaxRangesAsString))
                return Array.Empty<ServoRelaxRange>();

            return relaxRangesAsString
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(range =>
                {
                    var values = range.Split('-', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    if (values.Length != 2)
                        throw new FormatException($"Invalid relax range format: {range}");
                    return new ServoRelaxRange
                    {
                        MinValue = int.Parse(values[0]),
                        MaxValue = int.Parse(values[1])
                    };
                })
                .ToArray();
        }

        public static string ToString(ServoRelaxRange[] relaxRanges)
            => string.Join(",", relaxRanges.Select(r => $"{r.MinValue}-{r.MaxValue}"));
    }
}
