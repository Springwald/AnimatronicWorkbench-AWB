// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2025 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License

namespace Awb.Core.Project
{
    public abstract class AwbProjectFileVersion
    {
        /// <summary>
        /// The actual version of the file format for the awbprj file.
        /// </summary>
        /// <remarks>
        /// 001 = Version 0.01
        /// 100 = Version 1.00
        /// 1012 = version 10.12
        /// </remarks>
        public const int ActualProjectFileVersion = 1;

        /// <summary>
        /// The loades/saved version of the file format for the awbprj file.
        /// </summary>
        /// <remarks>
        /// 001 = Version 0.01
        /// 100 = Version 1.00
        /// 1012 = version 10.12
        /// </remarks>
        public int FileVersion { get; set; } = ActualProjectFileVersion;
    }
}
