﻿// Animatronic WorkBench core routines
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License


using Awb.Core.Export.ExporterParts;

namespace Awb.Core.Export
{
    public class Esp32ClientRemoteM5StickJoyCExporter : IExporter
    {
        private readonly string _esp32ClientsSourceFolder;
        private readonly WifiConfigExportData _wifiConfigData;

        public string Title { get; } = "ESP32 Remote Controller - M5Stack Mini JoyC HAT";

        public event EventHandler<ExporterProcessStateEventArgs>? Processing;

        public Esp32ClientRemoteM5StickJoyCExporter(string esp32ClientsSourceFolder, WifiConfigExportData wifiConfigData)
        {
            _esp32ClientsSourceFolder = esp32ClientsSourceFolder;
            _wifiConfigData = wifiConfigData;
        }

        public async Task<IExporter.ExportResult> ExportAsync(string targetPath)
        {
            var remoteSrcFolder = Path.Combine(_esp32ClientsSourceFolder, "awb_esp32_remote-M5Stick-Mini-JoyC-HAT");

            if (!Directory.Exists(remoteSrcFolder))
            {
                return new IExporter.ExportResult { ErrorMessage = $"Source folder '{remoteSrcFolder}' not found" };
            }

            var cloneResult = await new SrcFolderCloner(remoteSrcFolder, targetPath, removeExtraFilesInTarget: false).Clone();
            if (!cloneResult.Success)
            {
                Processing?.Invoke(this, new ExporterProcessStateEventArgs { ErrorMessage = cloneResult.ErrorMessage });
                return new IExporter.ExportResult { ErrorMessage = cloneResult.ErrorMessage };
            }

            var dataExporter = new[]
            {
                new WifiConfigExporter(_wifiConfigData),
            };

            foreach(var exporter in dataExporter)
            {
                var result = await exporter.ExportAsync(targetPath);
                if (result != IExporter.ExportResult.SuccessResult)
                {
                    Processing?.Invoke(this, new ExporterProcessStateEventArgs { ErrorMessage = result.ErrorMessage });
                    return result;
                }
            }   

            return IExporter.ExportResult.SuccessResult;
        }

    }
}
