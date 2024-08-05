﻿// Animatronic WorkBench core routines
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License


using Awb.Core.Export.ExporterParts;
using Awb.Core.Export.ExporterParts.CustomCode;

namespace Awb.Core.Export
{
    public class Esp32ClientRemoteM5StickJoyCExporter : MainExporterAbstract
    {
        private readonly WifiConfigExportData _wifiConfigData;

        public override string Title { get; } = "ESP32 Remote Controller - M5Stack Mini JoyC HAT";
        public override string TemplateSourceFolderRelative { get; } = "awb_esp32_remote-M5Stick-Mini-JoyC-HAT";

        public Esp32ClientRemoteM5StickJoyCExporter(string esp32ClientsTemplateSourceFolder, WifiConfigExportData wifiConfigData, string projectFolder) :
            base(projectFolder, esp32ClientsTemplateSourceFolder)
        {
            _wifiConfigData = wifiConfigData;
        }

        protected override ExporterPartAbstract[] GetExporterParts(string targetPath) => new ExporterPartAbstract[]
    {
            new WifiConfigExporter(_wifiConfigData, targetFolder:  Path.Combine(targetPath, "src", "AwbDataImport")),
            new CustomCodeExporter(customCodeRegionContent: CustomCodeRegionContent!, targetFolder: CustomCodeTargetFolder)
    };
    }
}
