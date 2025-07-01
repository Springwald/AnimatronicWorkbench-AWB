// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2025 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License

namespace Awb.Core.Sounds
{
    public class SoundManager
    {
        private readonly string _projectSoundPath;

        private Sound[]? _sounds;

        public Sound[] Sounds
        {
            get
            {
                if (this._sounds == null)
                    this._sounds = this.LoadSounds();
                return this._sounds!;
            }
        }

        public SoundManager(string projectSoundPath)
        {
            var possibleSubPaths = new[] { "sdcard", "01" };
            foreach (var subPath in possibleSubPaths)
            {
                projectSoundPath = Directory.Exists(Path.Combine(projectSoundPath, subPath)) ? Path.Combine(projectSoundPath, subPath) : projectSoundPath;
            }
            _projectSoundPath = projectSoundPath;
        }

        private Sound[] LoadSounds()
        {
            if (Directory.Exists(_projectSoundPath) == false) return Array.Empty<Sound>();

            var files = Directory.GetFiles(_projectSoundPath, "*.mp3");
            var sounds = new Sound[files.Length];
            for (var i = 0; i < files.Length; i++)
            {
                sounds[i] = new Sound(files[i]);
            }
            var doubletteQuery = sounds.GroupBy(x => x.Id)
              .Where(g => g.Count() > 1)
              .Select(y => y.Key)
              .ToList();

            if (doubletteQuery.Count > 0)
                throw new ArgumentException($"Sound Ids in sounds folders have to be unique. But the following Ids are exist more than once: {string.Join(", ", doubletteQuery)}");

            return sounds.OrderBy(s => s.Id).ToArray();
        }
    }
}
