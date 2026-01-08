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

        public IEnumerable<IGrouping<string, ProjectFile>> GetGroupedProjects()
            => Projects.GroupBy(p => p.Category);
    }
}
