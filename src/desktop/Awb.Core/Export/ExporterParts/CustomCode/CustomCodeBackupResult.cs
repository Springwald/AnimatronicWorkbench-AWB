// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2025 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License

namespace Awb.Core.Export.ExporterParts.CustomCode
{
    public class CustomCodeBackupResult
    {
        public required bool Success { get; init; }
        public CustomCodeRegionContent? CustomCodeRegionContent { get; init; }
        public string? ErrorMsg { get; init; }
    }
}
