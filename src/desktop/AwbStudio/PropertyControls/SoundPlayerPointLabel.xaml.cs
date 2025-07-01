// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2025 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License

using Awb.Core.Sounds;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace AwbStudio.PropertyControls
{
    public partial class SoundPlayerPointLabel : UserControl
    {
        private const int ImgHeight = 100;
        public string? LabelText
        {
            get => SoundPlayerLabel.Content?.ToString();
            set
            {
                SoundPlayerLabel.Content = value;
            }
        }

        public void SetSoundContent(Sound? sound)
        {
            ImageSource imageSource = GetSoundWaveImage(sound);
            WaveImage.Source = imageSource;
        }

        private static ImageSource GetSoundWaveImage(Sound? sound)
        {
            var imgWidth = sound?.DurationMs / 5 ?? 1000;
            var bitmap = new WriteableBitmap(pixelWidth: imgWidth, pixelHeight: ImgHeight, 96, 96, PixelFormats.Bgra32, null);
            FillBitmap(bitmap, sound);
            return bitmap;
        }

        private static void FillBitmap(WriteableBitmap bitmap, Sound? sound)
        {
            if (sound == null) return;

            var imgWidth = bitmap.PixelWidth;

            var bytesPerPixel = 4; // required for code below
            var stride = imgWidth * bytesPerPixel;
            var pixels = new byte[ImgHeight * stride];

            var samples = sound.Samples; // get the samples from the sound
            var factor = samples.Length / (double)imgWidth; // how many samples per pixel

            for (int x = 0; x < imgWidth; x++)
            {
                var sampleIndex = (int)(x * factor);
                sampleIndex = Math.Min(sampleIndex, samples.Length - 1); // ensure we don't go out of bounds

                var value = (samples[sampleIndex] * ImgHeight) / 512;
                var middle = ImgHeight / 2;

                int z = x * bytesPerPixel;
                pixels[z + 2] = 255; // R
                pixels[z + 3] = 255; // A

                for (int y = 0; y < value; y++)
                {
                    if (y < value)
                    {
                        int i = x * bytesPerPixel + (middle - y) * stride;
                        pixels[i] = 255; // B
                        pixels[i + 1] = 255; // G
                        pixels[i + 3] = 255; // A

                        i = x * bytesPerPixel + (middle + y) * stride;
                        pixels[i] = 255; // B
                        pixels[i + 1] = 255; // G
                        pixels[i + 3] = 255; // A
                    }
                }
            }

            var rect = new Int32Rect(0, 0, imgWidth, ImgHeight);
            bitmap.WritePixels(rect, pixels, stride, 0);
            bitmap.Freeze(); // make it immutable for performance
        }

        public void SetWidthByDuration(double widthInPixel)
        {
            SoundPlayerLabel.Width = widthInPixel;
            WaveImage.Width = widthInPixel;
        }

        public SoundPlayerPointLabel()
        {
            InitializeComponent();
        }
    }
}
