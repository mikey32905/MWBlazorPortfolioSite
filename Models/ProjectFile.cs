namespace MWBlazorPortfolioSite.Models
{
    public class ProjectFile
    {
        public string FileName { get; set; } = string.Empty;
        public string Extension { get; set; } = ".razor"; // .xaml, .cs, etc.
        public string Category { get; set; } = "Blazor"; // For grouping
        public string AccentColor { get; set; } = "#00F3FF";
        public string Icon { get; set; } = "📄"; 
        public string Description { get; set; } = string.Empty;
    }
}
