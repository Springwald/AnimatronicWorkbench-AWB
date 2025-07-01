// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2025 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License

namespace AwbStudio.Projects
{
    public record OpenProjectResult
    {
        public required bool Success { get; init; }
        public required string[] ErrorMessages { get; init; }

        public static OpenProjectResult SuccessResult => new() { Success = true, ErrorMessages = [] };
        public static OpenProjectResult ErrorResult(string[] errorMessages) => new() { Success = false, ErrorMessages = errorMessages };
    }
}
