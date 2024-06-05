// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using System;

namespace AwbStudio
{
    /// <summary>
    /// holds the main configuration of the application.
    /// Only root information like version number, support-Urls, etc..
    /// No user specific data, no project specific data or path information.
    /// </summary>
    /// <remarks>
    /// todo: implement as injectable service
    /// </remarks>
    internal static class MainConfig
    {
        public const string Version = "0.4.0";
        public static DateTime VersionReleaseDate = new DateTime(2024, 5, 6);
        public static string VersionLabelInfo => $"Version {Version} ({VersionReleaseDate:yyyy MMMM dd})";

        public static bool ProjectConfigEditorAvailable = false; // todo: set to true when the project config editor is available, planned for future releases

    }
}
