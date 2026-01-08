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

        [JsonPropertyName("LiveUrl")]
        public string LiveUrl { get; set; } = "";

        [JsonPropertyName("AccentColor")]
        public string AccentColor { get; set; } = "#00FF41";

        [JsonPropertyName("Icon")]
        public string Icon { get; set; } = "📄";

        [JsonPropertyName("Description")]
        public string Description { get; set; } = "";
    }
}
