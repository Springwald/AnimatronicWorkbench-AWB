// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2025 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License

using System;

namespace AwbStudio.StudioSettings
{
    /// <summary>
    /// holds the main configuration of the application.
    /// Only root information like version number, support-Urls, etc..
    /// No user specific data, no project specific data or path information.
    /// </summary>
    internal class VersionInfo
    {
        public string Version { get; set; }

        public DateOnly VersionReleaseDate { get; set; }

        public VersionInfo(string version, DateOnly versionReleaseDate)
        {
            if (string.IsNullOrWhiteSpace(version))
                throw new ArgumentException("Version cannot be null or empty.", nameof(version));
            Version = version;
            VersionReleaseDate = versionReleaseDate;
        }

        /// <summary>
        /// Returns the name of the embedded JSON file that contains the version information.
        /// </summary>
        private static string EmbeddedJsonFileName => "AwbStudio.StudioSettings.Version.json";

        /// <summary>
        /// Gets the URL of the GitHub resource containing the actual version information for the application.
        /// </summary>
        private static string GitHubAcutalVersionUrl =>
            "https://raw.githubusercontent.com/Springwald/AnimatronicWorkbench-AWB/refs/heads/main/src/desktop/AwbStudio/StudioSettings/Version.json";

        /// <summary>
        /// Retrieves version information from a predefined GitHub URL.
        /// </summary>
        public static VersionInfo? ReadFromGitHub()
        {
            try
            {
                // Reads the version information from a GitHub URL and returns a VersionInfo object.
                using var client = new System.Net.Http.HttpClient();
                var response = client.GetStringAsync(GitHubAcutalVersionUrl).Result;
                return System.Text.Json.JsonSerializer.Deserialize<VersionInfo>(response)
                       ?? throw new InvalidOperationException("Failed to deserialize version info from GitHub.");
            }
            catch (Exception ex)
            {
                // Log the exception or handle it as needed.
                Console.WriteLine($"Error reading version info from GitHub: {ex.Message}");
                return null;
            }
        }
        public static VersionInfo ReadFromEmbeddedJson()
        {
            // Reads the version information from an embedded JSON file and returns a VersionInfo object.
            var json = System.Reflection.Assembly.GetExecutingAssembly()
                .GetManifestResourceStream(EmbeddedJsonFileName);
            if (json == null)
                throw new InvalidOperationException($"Embedded JSON file '{EmbeddedJsonFileName}' not found.");
            using var reader = new System.IO.StreamReader(json);
            var content = reader.ReadToEnd();
            return System.Text.Json.JsonSerializer.Deserialize<VersionInfo>(content)
                   ?? throw new InvalidOperationException("Failed to deserialize version info from embedded JSON.");
        }
    }
}
