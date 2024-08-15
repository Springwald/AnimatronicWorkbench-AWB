// Animatronic WorkBench core routines
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using System.Text;
using System.Text.RegularExpressions;
using static Awb.Core.Export.ExporterParts.CustomCode.CustomCodeRegionContent;

namespace Awb.Core.Export.ExporterParts.CustomCode
{
    public class CustomCodeReaderWriter
    {
        private static readonly Regex _regionsRegex = new Regex(@"\/\*[\s]+cc-(?<regionstartend>[a-z]+)-(?<regionkey>[a-z]+).*?\*\/", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public event EventHandler<ExporterProcessStateEventArgs>? Processing;

        public record RegionsReadResult
        {
            public bool Success => string.IsNullOrEmpty(ErrorMsg);
            public required Region[] Regions;
            public required string? ErrorMsg;
        }

        public record RegionsWriteResult
        {
            public bool Success => string.IsNullOrEmpty(ErrorMsg);
            public required string? Content;
            public required string? ErrorMsg;
        }

        /// <summary>
        /// Read the content of the regions from a custom code file
        /// </summary>
        public RegionsReadResult ReadRegions(string filename, string content)
        {
            Processing?.Invoke(this, new ExporterProcessStateEventArgs { State = ExporterProcessStateEventArgs.ProcessStates.OnlyLog, Message = $"\r\n----------------------------------------------------" });
            Processing?.Invoke(this, new ExporterProcessStateEventArgs { State = ExporterProcessStateEventArgs.ProcessStates.OnlyLog, Message = $"## Read regions from '{filename}'"});

            var lines = content.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
            var regionContent = new StringBuilder();
            var regions = new List<Region>();
            string? actualRegionKey = null;
            foreach (var line in lines)
            {
                var match = _regionsRegex.Match(line);
                if (match.Success)
                {
                    var lineType = match.Groups["regionstartend"].Value;
                    var regionKey = match.Groups["regionkey"].Value;
                    switch (lineType)
                    {
                        case "start":
                            if (actualRegionKey != null)
                                return new RegionsReadResult { ErrorMsg = $"Region '{actualRegionKey}' not closed", Regions = Array.Empty<Region>() };
                            actualRegionKey = regionKey;
                            break;
                        case "end":
                            if (actualRegionKey == null)
                                return new RegionsReadResult { ErrorMsg = $"Region '{regionKey}' not started", Regions = Array.Empty<Region>() };
                            if (actualRegionKey != regionKey)
                                return new RegionsReadResult { ErrorMsg = $"Region '{actualRegionKey}' not closed", Regions = Array.Empty<Region>() };

                            var contentCleandUp = regionContent.ToString().Trim(new char[] { '\r', '\n', ' ', '\t' });
                            if (!string.IsNullOrWhiteSpace(contentCleandUp))
                                regions.Add(new Region { Filename = filename, Key = regionKey, Content = contentCleandUp });
                            actualRegionKey = null;
                            break;

                        default:
                            return new RegionsReadResult { ErrorMsg = $"Unknown region key '{regionKey}'", Regions = Array.Empty<Region>() };
                    }
                    regionContent.Clear();
                }
                else
                {
                    regionContent.AppendLine(line);
                }
            }

            Processing?.Invoke(this, new ExporterProcessStateEventArgs { State = ExporterProcessStateEventArgs.ProcessStates.OnlyLog, Message = $"## Read {regions.Count} regions from '{filename}'" });
            foreach (var region in regions)
            {
                Processing?.Invoke(this, new ExporterProcessStateEventArgs { State = ExporterProcessStateEventArgs.ProcessStates.OnlyLog, Message = $"\r\n------------------------------------" });
                Processing?.Invoke(this, new ExporterProcessStateEventArgs { State = ExporterProcessStateEventArgs.ProcessStates.OnlyLog, Message = $"-> Region '{region.Key}' from '{filename}'" });
                Processing?.Invoke(this, new ExporterProcessStateEventArgs { State = ExporterProcessStateEventArgs.ProcessStates.OnlyLog, Message = $"------------------------------------" });
                Processing?.Invoke(this, new ExporterProcessStateEventArgs { State = ExporterProcessStateEventArgs.ProcessStates.OnlyLog, Message = region.Content });
            }

            return new RegionsReadResult { Regions = regions.ToArray(), ErrorMsg = null };

        }


        /// <summary>
        /// replace the content of the regions in the custom code template file with the custom code content
        /// </summary>
        public RegionsWriteResult WriteRegions(string filename, string templateContent, CustomCodeRegionContent regionContent)
        {
            Processing?.Invoke(this, new ExporterProcessStateEventArgs { State = ExporterProcessStateEventArgs.ProcessStates.OnlyLog, Message = $"\r\n-------------------------------------------------" });
            Processing?.Invoke(this, new ExporterProcessStateEventArgs { State = ExporterProcessStateEventArgs.ProcessStates.OnlyLog, Message = $"## Write regions to '{filename}'" });

            var result = new StringBuilder();
            var lines = templateContent.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
            var regions = new List<Region>();
            string? actualRegionKey = null;
            foreach (var line in lines)
            {
                var match = _regionsRegex.Match(line);
                if (match.Success)
                {
                    var lineType = match.Groups["regionstartend"].Value;
                    var regionKey = match.Groups["regionkey"].Value;
                    switch (lineType)
                    {
                        case "start":
                            if (actualRegionKey != null)
                                return new RegionsWriteResult { ErrorMsg = $"Region '{actualRegionKey}' not closed", Content = null };
                            if (string.IsNullOrEmpty(regionKey))
                                return new RegionsWriteResult { ErrorMsg = $"Region key is empty", Content = null };

                            Processing?.Invoke(this, new ExporterProcessStateEventArgs { State = ExporterProcessStateEventArgs.ProcessStates.OnlyLog, Message = $"------------------------------------" });
                            Processing?.Invoke(this, new ExporterProcessStateEventArgs { State = ExporterProcessStateEventArgs.ProcessStates.OnlyLog, Message = $"Start region '{regionKey}' from '{filename}'" });

                            actualRegionKey = regionKey;
                            result.AppendLine(line);
                            var content = regionContent.Regions.Where(r => r.Filename == filename && r.Key == regionKey).Select(r => r.Content).ToArray();
                            switch (content.Length)
                            {
                                case 0:
                                    // no content for this region read from the custom code file
                                    Processing?.Invoke(this, new ExporterProcessStateEventArgs { State = ExporterProcessStateEventArgs.ProcessStates.OnlyLog, Message = $"No content for region '{regionKey}'" });
                                    break;
                                case 1:
                                    // content for this region read from the custom code file
                                    result.AppendLine(content[0]);
                                    Processing?.Invoke(this, new ExporterProcessStateEventArgs { State = ExporterProcessStateEventArgs.ProcessStates.OnlyLog, Message = $"written Content for region '{regionKey}':" });
                                    Processing?.Invoke(this, new ExporterProcessStateEventArgs { State = ExporterProcessStateEventArgs.ProcessStates.OnlyLog, Message = content[0] });
                                    break;
                                default:
                                    return new RegionsWriteResult { ErrorMsg = $"Multiple regions with the same key '{regionKey}'", Content = null };
                            }
                            break;
                        case "end":
                            actualRegionKey = null;
                            Processing?.Invoke(this, new ExporterProcessStateEventArgs { State = ExporterProcessStateEventArgs.ProcessStates.OnlyLog, Message = $"End region '{regionKey}' from '{filename}'" });
                            break;

                        default:
                            return new RegionsWriteResult { ErrorMsg = $"Unknown region key '{regionKey}'", Content = null };
                    }
                }
                if (actualRegionKey == null)
                    result.AppendLine(line);
            }

            return new RegionsWriteResult { Content = result.ToString(), ErrorMsg = null };
        }
    }
}
