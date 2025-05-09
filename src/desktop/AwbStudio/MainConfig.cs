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
        public const string Version = "0.8.1";
        public static DateTime VersionReleaseDate = new DateTime(2025, 05, 09);

        public static bool TestMode = false; // set to false for production before pull request for release
        //public static bool TestMode = Debugger.IsAttached; // remove this line for production 

    }
}
