// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2025 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License

namespace Awb.Core.Export
{
    public interface IExporter
    {
        public class ExportResult
        {
            public bool Success => string.IsNullOrEmpty(ErrorMessage);
            public string? ErrorMessage { get; set; }

            public static ExportResult SuccessResult => new ExportResult();
        }

        string Title { get; }

        // reports the current state as event
        event EventHandler<ExporterProcessStateEventArgs>? ProcessingState;

        // exports the data to the targetPath
        Task<ExportResult> ExportAsync(string targetPath);
    }
}
