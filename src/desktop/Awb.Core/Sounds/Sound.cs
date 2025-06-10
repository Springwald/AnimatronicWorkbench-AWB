// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2025 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License

namespace Awb.Core.Sounds
{
    public class Sound
    {
        public const int SamplesPerSecond = 500; // standard for MP3 files
        public byte[] _samples;

        public string Mp3Filename { get; }

        public int Id { get; }

        public string Title { get; }
        public int DurationMs { get; set; }

        public byte[] Samples
        {
            get
            {
                if (_samples == null)
                {
                    // lazy loading

                    // read and rescale
                    var samplesPure = ReadSamples();
                    var newSamplesLength = (int)((DurationMs / 1000.0) * SamplesPerSecond);
                    var factor = samplesPure.Length / newSamplesLength;
                    var samples = new byte[newSamplesLength];

                    for (int i = 0; i < newSamplesLength; i++)
                    {
                        int from = (int)(i * factor);
                        int to = (int)((i + 1) * factor) - 1;
                        // get  the max value from the range
                        float max = 0f;
                        for (int j = from; j <= to; j++)
                        {
                            if (j < 0 || j >= samplesPure.Length) continue;
                            if (Math.Abs(samplesPure[j]) > max)
                                max = Math.Abs(samplesPure[j]);
                        }
                        samples[i] = (byte)(max * 255); // rescale to 0..1 to 0..255
                    }
                    _samples = samples;
                }
                return _samples;
            }
        }

        public Sound(string mp3Filename)
        {
            Mp3Filename = mp3Filename;

            if (string.IsNullOrWhiteSpace(Mp3Filename)) throw new ArgumentNullException(nameof(mp3Filename));
            if (!File.Exists(mp3Filename)) throw new FileNotFoundException("MP3 file not found", mp3Filename);

            var pureFilename = Path.GetFileNameWithoutExtension(mp3Filename);

            //extract the mp3 file ID
            if (pureFilename.Length < 3) throw new ArgumentException("mp3 filename is not in format 001ABC.M3", nameof(pureFilename));
            var id = pureFilename.Substring(0, 3);
            if (!int.TryParse(id, out var idInt)) throw new ArgumentException("mp3 filename is not in format 001ABC.M3", nameof(pureFilename));

            Id = idInt;

            // extract the title
            var title = pureFilename.Substring(3);
            if (string.IsNullOrWhiteSpace(title)) title = pureFilename;
            Title = title;

            // get the duration
            TagLib.File file = TagLib.File.Create(mp3Filename);
            DurationMs = (int)file.Properties.Duration.TotalMilliseconds;




        }

        private float[] ReadSamples()
        {
            // Load the audio file and read samples
            var filename = Mp3Filename;
            byte[] data = File.ReadAllBytes(filename);
            using var memStream = new System.IO.MemoryStream(data);
            using var mpgFile = new NLayer.MpegFile(memStream);
            var chanels = mpgFile.Channels;

            var samplesCount = (int)(mpgFile.Length / 2);//  explorative: 2 bytes per sample (16 bit PCM) TODO: check if this is correct for MP3 files!
            //var samplesCount =  (int)(mpgFile.Duration.TotalSeconds * (mpgFile.SampleRate * chanels)); // alternative: calculate samples count based on duration and sample rate (NOT WORKING!)
            mpgFile.StereoMode = NLayer.StereoMode.DownmixToMono;
            var samples = new float[samplesCount];
            mpgFile.ReadSamples(samples, 0, samplesCount);
            return samples;
        }
    }
}
