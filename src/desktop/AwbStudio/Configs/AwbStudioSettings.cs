// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2023 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using System.Linq;

namespace AwbStudio.Configs
{
    public class AwbStudioSettings
    {
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
