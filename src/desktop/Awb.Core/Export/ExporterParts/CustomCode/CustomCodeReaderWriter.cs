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
    internal class CustomCodeReaderWriter
    {
        private static readonly Regex _regionsRegex = new Regex(@"\/\*[\s]+cc-(?<regionstartend>[a-z]+)-(?<regionkey>[a-z]+).*?\*\/", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public record RegionsReadResult
        {
            public required Region[] Regions;
            public required string? ErrorMsg;
        }

        public record RegionsWriteResult
        {
            public required string? Content;
            public required string? ErrorMsg;
        }

        /// <summary>
        /// Read the content of the regions from a custom code file
        /// </summary>
        public RegionsReadResult ReadRegions(string filename, string content)
        {
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

                            regions.Add(new Region { Filename = filename, Key = regionKey, Content = regionContent.ToString() });
                            actualRegionKey = null;
                            regionContent.Clear();
                            break;

                        default:
                            return new RegionsReadResult { ErrorMsg = $"Unknown region key '{regionKey}'", Regions = Array.Empty<Region>() };
                    }
                }
                else
                {
                    regionContent.AppendLine(line);
                }
            }
            return new RegionsReadResult { Regions = regions.ToArray(), ErrorMsg = null };

        }


        /// <summary>
        /// replace the content of the regions in the custom code template file with the custom code content
        /// </summary>
        public RegionsWriteResult WriteRegions(string templateContent, CustomCodeRegionContent regionContent)
        {
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

                            actualRegionKey = regionKey;
                            result.AppendLine(line);
                            var content = regionContent.Regions.Where(r => r.Key == regionKey).Select(r => r.Content).ToArray();
                            switch (content.Length)
                            {
                                case 0:
                                    // no content for this region read from the custom code file
                                    break;
                                case 1:
                                    // content for this region read from the custom code file
                                    result.AppendLine(content[0]);
                                    break;
                                default:
                                    return new RegionsWriteResult { ErrorMsg = $"Multiple regions with the same key '{regionKey}'", Content = null };
                            }
                            break;
                        case "end":
                            actualRegionKey = null;
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
