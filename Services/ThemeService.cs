namespace MWBlazorPortfolioSite.Services
{
    public class ThemeService
    {
        public string CurrentColor { get; set; } = "#00F3FF";
        public event Action? OnThemeChanged;

        public void UpdateTheme(string newHex)
        {
            CurrentColor = newHex;
            OnThemeChanged?.Invoke();
        }

        // Helper to convert Hex to RGB for CSS rgba() functions
        public string GetRgbValues()
        {
            // Simple logic to convert #RRGGBB to "R, G, B"
            var color = System.Drawing.ColorTranslator.FromHtml(CurrentColor);
            return $"{color.R}, {color.G}, {color.B}";
        }
    }
}
