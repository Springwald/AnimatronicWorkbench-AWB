// Animatronic WorkBench core routines
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using System.Text;

namespace Awb.Core.Export.ExporterParts
{
    abstract class ExporterPartAbstract
    {
        public abstract Task<IExporter.ExportResult> ExportAsync(string targetSrcFolder);

        protected string GetHeader(string className, string includes)
        {
            var content = new StringBuilder();
            content.AppendLine($"#ifndef _{className.ToUpper()}_H_");
            content.AppendLine($"#define _{className.ToUpper()}_H_");
            content.AppendLine();
            content.AppendLine(includes);
            content.AppendLine();
            content.AppendLine("// Created with Animatronic Workbench Studio");
            content.AppendLine("// https://daniel.springwald.de/post/AnimatronicWorkbench");
            content.AppendLine();
            content.AppendLine($"// Created on {DateTime.Now}");
            content.AppendLine();
            content.AppendLine($"class {className}");
            content.AppendLine("{");
            
            return content.ToString();
        }

        protected string GetFooter(string className)
        {
            var content = new StringBuilder();
            content.AppendLine();
            content.AppendLine("};");
            content.AppendLine();
            content.AppendLine($"#endif // _{className.ToUpper()}_H_");
            return content.ToString();
        }
    }
}
