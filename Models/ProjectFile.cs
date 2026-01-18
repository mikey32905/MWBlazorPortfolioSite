using System.Text.Json.Serialization;

namespace MWBlazorPortfolioSite.Models
{
    public class ProjectFile
    {
        [JsonPropertyName("Id")]
        public string Id { get; set; } = "";

        [JsonPropertyName("FileName")]
        public string FileName { get; set; } = "";
        public string ModuleName { get; set; }
        public string Status { get; set;  }
        public List<string> CoreTech { get; set; } = new List<string>();

        [JsonPropertyName("Extension")]
        public string Extension { get; set; } = "";

        [JsonPropertyName("Category")]
        public string Category { get; set; } = "GENERAL";

        [JsonPropertyName("PhysicalPath")]
        public string PhysicalPath { get; set; } = "";

        [JsonPropertyName("Language")]
        public string Language { get; set; } = "csharp";

        [JsonPropertyName("LiveUrl")]
        public string LiveUrl { get; set; } = "";

        [JsonPropertyName("GitHubUrl")]
        public string? GitHubUrl { get; set; }

        [JsonPropertyName("AccentColor")]
        public string AccentColor { get; set; } = "#00FF41";

        [JsonPropertyName("Icon")]
        public string Icon { get; set; } = "📄";

        [JsonPropertyName("Description")]
        public List<string> Description { get; set; } = new List<string>();
        // The relative path to the screenshot (e.g., "images/blueprints/TitanVault.png")

        [JsonPropertyName("BlueprintUrl")]
        public string? BlueprintUrl { get; set; }

        // The path to the raw XAML text file
        [JsonPropertyName("SourceUrl")]
        public string? SourceUrl { get; set; }

        public string? DownloadUrl { get; set; }
        public string? Version { get; set; } = "v1.0.0";
        public int LineCount { get; set; } // The "Neural Weight" of the module
        // This allows the JSON: "Metadata": { "KEY": "VALUE" } to be mapped automatically
        public Dictionary<string, string> Metadata { get; set; } = new();

        // Key = Tab Name (e.g., "XAML", "C#"), Value = SourceUrl
        public Dictionary<string, string> SupportingFiles { get; set; } = new();
    }
}
