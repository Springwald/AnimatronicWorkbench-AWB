// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2025 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License


using Awb.Core.Export.ExporterParts;
using Awb.Core.Export.ExporterParts.CustomCode;
using Awb.Core.Export.ExporterParts.ExportData;

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
            new CustomCodeExporter(customCodeRegionContent: CustomCodeRegionContent!, targetFolder:  CustomCodeTargetFolder ?? throw new Exception("CustomCodeTargetFolder not set"))
        };
    }
}
