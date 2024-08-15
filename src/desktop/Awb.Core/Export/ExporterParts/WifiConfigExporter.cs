// Animatronic WorkBench core routines
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using System.Text;
using Awb.Core.Export.ExporterParts.ExportData;

namespace Awb.Core.Export.ExporterParts
{
    internal class WifiConfigExporter : ExporterPartAbstract
    {
        private readonly WifiConfigExportData _wifiConfigData;
        private readonly string _targetFolder;

        public WifiConfigExporter(WifiConfigExportData wifiConfigData, string targetFolder)
        {
            _wifiConfigData = wifiConfigData;
            _targetFolder = targetFolder;
        }

        public override async Task<IExporter.ExportResult> ExportAsync()
        {
            var content = new StringBuilder();
            content.AppendLine(GetHeader(className: "WifiConfig", includes: string.Empty));

            content.AppendLine("public:");
            content.AppendLine($"   const char *WlanSSID = \"{_wifiConfigData.WlanSSID}\";");
            content.AppendLine($"   const char *WlanPassword = \"{_wifiConfigData.WlanPassword}\";");

            content.AppendLine(GetFooter("WifiConfig"));

            if (!Directory.Exists(_targetFolder))
                return new IExporter.ExportResult { ErrorMessage = $"Target folder '{_targetFolder}' not found" };

            await File.WriteAllTextAsync(Path.Combine(_targetFolder, "WifiConfig.h"), content.ToString());


            return IExporter.ExportResult.SuccessResult;
        }

    }
}
