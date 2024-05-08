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
        private readonly WifiConfigExportData _wifiConfigData;

        public WifiConfigExporter(WifiConfigExportData wifiConfigData)
        {
            _wifiConfigData = wifiConfigData;
        }

        public override async Task<IExporter.ExportResult> ExportAsync(string targetSrcFolder)
        {
            var content = new StringBuilder();
            content.AppendLine(GetHeader(className: "WifiConfig", includes: string.Empty));

            content.AppendLine("public:");
            content.AppendLine($"   const char *WlanSSID = \"{_wifiConfigData.WlanSSID}\";");
            content.AppendLine($"   const char *WlanPassword = \"{_wifiConfigData.WlanPassword}\";");

            content.AppendLine(GetFooter("WifiConfig"));

            if (!Directory.Exists(targetSrcFolder))
                return new IExporter.ExportResult { ErrorMessage = $"Target folder '{targetSrcFolder}' not found" };

            var folder = Path.Combine(targetSrcFolder, "src", "AwbDataImport");
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            await File.WriteAllTextAsync(Path.Combine(folder, "WifiConfig.h"), content.ToString());


            return IExporter.ExportResult.SuccessResult;
        }

    }
}
