// Animatronic WorkBench core routines
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using System.Text;

namespace Awb.Core.Export.ExporterParts
{
    internal class WifiConfigExporter : ExporterPartAbstract
    {
        private readonly WifiConfigData _wifiConfigData;

        public WifiConfigExporter(WifiConfigData wifiConfigData)
        {
            _wifiConfigData = wifiConfigData;
        }

        public override async Task<IExporter.ExportResult> ExportAsync(string targetSrcFolder)
        {
            var content = new StringBuilder();
            content.AppendLine(GetHeader("WifiConfig"));

            content.AppendLine("public:");
            content.AppendLine($"   const char *WlanSSID = \"{_wifiConfigData.WlanSSID}\";");
            content.AppendLine($"   const char *WlanPassword = \"{_wifiConfigData.WlanPassword}\";");

            content.AppendLine(GetFooter("WifiConfig"));

            if (!Directory.Exists(Path.Combine(targetSrcFolder, "src")))
                Directory.CreateDirectory(Path.Combine(targetSrcFolder, "src"));

            File.WriteAllText(Path.Combine(targetSrcFolder, "src", "WifiConfig.h"), content.ToString());

            return IExporter.ExportResult.SuccessResult;
        }

    }
}
