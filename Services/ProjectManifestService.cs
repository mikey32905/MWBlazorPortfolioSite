using MWBlazorPortfolioSite.Models;
using System.Net.Http.Json;

namespace MWBlazorPortfolioSite.Services
{
    public class ProjectManifestService
    {
        private readonly HttpClient _http;
        public List<ProjectFile> Projects { get; private set; } = new();

        public event Action? OnManifestLoaded;

        public ProjectManifestService(HttpClient http)
        {
            _http = http;
        }

        public async Task InitializeAsync()
        {
            try
            {
                // Fetch the JSON from the wwwroot folder
                var data = await _http.GetFromJsonAsync<List<ProjectFile>>("Data/projects.json");
                if (data != null)
                {
                    Projects = data;
                    OnManifestLoaded?.Invoke();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Critical Error loading manifest: {ex.Message}");
            }
        }

        public int GetTotalNeuralWeight() => Projects.Sum(p => p.LineCount);

        public string GetFormattedNeuralWeight()
        {
            int total = GetTotalNeuralWeight();
            return total >= 1000 ? $"{(total / 1000.0):F1}K" : total.ToString();
        }

        public string GetSystemComplexityScore()
        {
            int score = 0;
            foreach (var project in Projects)
            {
                // Assign weights based on category
                score += project.Category.ToUpper() switch
                {
                    "WPF_CORE" => 50,
                    "LIVE_APPS" => 35,
                    "SYSTEM_CORE" => 60,
                    _ => 20
                };
            }

            return score switch
            {
                > 400 => "CRITICAL_MASS",
                > 250 => "ADVANCED_MATRIX",
                > 100 => "STABLE_BUILD",
                _ => "DEVELOPMENT_PHASE"
            };
        }

        public IEnumerable<IGrouping<string, ProjectFile>> GetGroupedProjects()
            => Projects.GroupBy(p => p.Category);
    }
}
