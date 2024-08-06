// Animatronic WorkBench core routines
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License


using Awb.Core.Export.ExporterParts;
using Awb.Core.Export.ExporterParts.CustomCode;

namespace Awb.Core.Export
{
    public class Esp32ClientExporter : MainExporterAbstract
    {
        private readonly WifiConfigExportData _wifiConfigData;
        private readonly ProjectExportData _projectExportData;

        public override string Title { get; } = "ESP32 Client";
        public override string TemplateSourceFolderRelative { get; } = "awb_esp32_client";



        public Esp32ClientExporter(string esp32ClientsTemplateSourceFolder, WifiConfigExportData wifiConfigData, ProjectExportData projectExportData, string projectFolder) :
            base(projectFolder, esp32ClientsTemplateSourceFolder)
        {
            _wifiConfigData = wifiConfigData;
            _projectExportData = projectExportData;
        }

        protected override ExporterPartAbstract[] GetExporterParts(string targetPath)
            => new ExporterPartAbstract[]
        {
            new WifiConfigExporter(_wifiConfigData, targetFolder:  Path.Combine(targetPath , "src", "AwbDataImport")),
            new ProjectDataExporter(_projectExportData, targetFolder:  Path.Combine(targetPath, "src", "AwbDataImport")),
            new CustomCodeExporter(customCodeRegionContent: CustomCodeRegionContent!, targetFolder:  CustomCodeTargetFolder!)
        };
    }
}
