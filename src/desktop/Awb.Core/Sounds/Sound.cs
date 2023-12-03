// Animatronic WorkBench core routines
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2023 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

namespace Awb.Core.Sounds
{
    public class Sound
    {
        public string Mp3Filename { get; }

        public int Id { get; }

        public int FolderId { get; } = 1;

        public string Title { get; }

        public Sound(string mp3Filename)
        {
            Mp3Filename = mp3Filename;

            if (string.IsNullOrWhiteSpace(Mp3Filename)) throw new ArgumentNullException(nameof(mp3Filename));
            if (!File.Exists(mp3Filename)) throw new FileNotFoundException("MP3 file not found", mp3Filename);

            var pureFilename = Path.GetFileNameWithoutExtension(mp3Filename);

            //extract the mp3 file ID
            if (pureFilename.Length < 3) throw new ArgumentException("mp3 filename is not in format 001ABC.M3", nameof(pureFilename));
            var folderId = pureFilename.Substring(0, 3);
            if (!int.TryParse(folderId, out var folderIdInt)) throw new ArgumentException("mp3 filename is not in format 001ABC.M3", nameof(pureFilename));
            FolderId = folderIdInt;

            // extract the title
            var title = pureFilename.Substring(3);
            Title = string.IsNullOrWhiteSpace(title) ? pureFilename : title;
        }

    }
}
