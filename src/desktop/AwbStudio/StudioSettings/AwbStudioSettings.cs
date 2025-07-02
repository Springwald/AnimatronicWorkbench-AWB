// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2025 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License

using System.Linq;

namespace AwbStudio.StudioSettings
{
    public class AwbStudioSettings
    {
        /// <summary>
        /// Should the last project be reopened on start
        /// </summary>
        public bool ReOpenLastProjectOnStart { get; set; } = false;


        /// <summary>
        /// Use dark mode for the UI
        /// </summary>
        public bool DarkMode { get; set; } = true;

        /// <summary>
        /// Which projects were opened last
        /// </summary>
        public string[] LatestProjectsFolders { get; set; } = new string[0];

        public void AddLastProjectFolder(string folder)
        {
            var folders = LatestProjectsFolders.ToList();

            folders.Insert(0, folder);
            LatestProjectsFolders = folders.Distinct().Take(10).ToArray();
        }
    }
}
