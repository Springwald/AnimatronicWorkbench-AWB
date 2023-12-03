// Animatronic WorkBench core routines
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2023 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

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

        private Sound[] LoadSounds()
        {
            if (System.IO.Directory.Exists(_projectSoundPath) == false) return Array.Empty<Sound>();
            var files = System.IO.Directory.GetFiles(_projectSoundPath, "*.mp3");
            var sounds = new Sound[files.Length];
            for (var i = 0; i < files.Length; i++)
            {
                sounds[i] = new Sound(files[i]);
            }
            return sounds;
        }

        public SoundManager(string projectSoundPath)
        {
            _projectSoundPath = projectSoundPath;

        }
    }
}
