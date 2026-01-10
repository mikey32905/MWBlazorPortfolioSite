using System.Text.Json.Serialization;

namespace MWBlazorPortfolioSite.Models
{
    public class ProjectFile
    {
        [JsonPropertyName("FileName")]
        public string FileName { get; set; } = "";

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

        [JsonPropertyName("AccentColor")]
        public string AccentColor { get; set; } = "#00FF41";

        [JsonPropertyName("Icon")]
        public string Icon { get; set; } = "📄";

        [JsonPropertyName("Description")]
        public string Description { get; set; } = "";
        // The relative path to the screenshot (e.g., "images/blueprints/TitanVault.png")
 
        [JsonPropertyName("BlueprintUrl")]
        public string? BlueprintUrl { get; set; }

        // The path to the raw XAML text file
        [JsonPropertyName("SourceUrl")]
        public string? SourceUrl { get; set; }

        // This allows the JSON: "Metadata": { "KEY": "VALUE" } to be mapped automatically
        public Dictionary<string, string> Metadata { get; set; } = new();
    }
}
