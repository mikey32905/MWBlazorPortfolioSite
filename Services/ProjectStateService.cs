using MWBlazorPortfolioSite.Models;
using System.Net.Http.Json;
using System.Text.Json;

namespace MWBlazorPortfolioSite.Services
{
    public class ProjectStateService
    {
        private readonly HttpClient _http; 
        public ProjectFile? SelectedFile { get; private set; }
        // The full list of all projects/files from your JSON
        public List<ProjectFile> ProjectFiles { get; set; } = new();

        // Add this line! This is what was missing.
        public string CurrentAccentColor { get; private set; } = "#00FF41";
        public string? SelectedFileContent { get; private set; } // Holds the raw XAML/C# text

        public event Action? OnChange;
        // The "Messenger": Components subscribe to this to know when to react
        public event Action? OnSelectedFileChanged;

        public ProjectStateService(HttpClient http)
        {
            _http = http;
        }

        public async Task InitializeAsync(HttpClient http)
        {
            Console.WriteLine(">>> FETCH_SEQUENCE_STARTING..."); // Check F12 Console for this!

            // FIX: Check if the list is null OR if it has 0 items
            if (ProjectFiles != null && ProjectFiles.Any())
            {
                Console.WriteLine($">>> SKIPPING: Already have {ProjectFiles.Count} files.");
                return;
            }

            try
            {
                // Force the fetch
                var result = await http.GetFromJsonAsync<List<ProjectFile>>("Data/projects.json");

                if (result != null)
                {
                    ProjectFiles = result;
                    Console.WriteLine($">>> FETCH_SUCCESS: {ProjectFiles.Count} files loaded.");
                    NotifyStateChanged();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($">>> FETCH_ERROR: {ex.Message}");
            }
        }

              public async Task SelectFile(ProjectFile? file)
        {
            SelectedFile = file;
            SelectedFileContent = null; // Clear old content immediately

            if (file != null)
            {
                CurrentAccentColor = file.AccentColor;

                // If the file has a SourceUrl, go fetch the actual code
                if (!string.IsNullOrEmpty(file.SourceUrl))
                {
                    try
                    {
                        SelectedFileContent = await _http.GetStringAsync(file.SourceUrl);
                    }
                    catch (Exception ex)
                    {
                        SelectedFileContent = $"// ERROR: Could not load source. {ex.Message}";
                    }
                }
            }
           

            NotifyStateChanged();
        }

        public async Task UpdateActiveContent(string url)
        {
            SelectedFileContent = null; // Clear old text
            NotifyStateChanged();

            try
            {
                // Use a slight delay to test if it's a race condition locally
                // await Task.Delay(100); 

                var content = await _http.GetStringAsync(url);
                SelectedFileContent = content; // Step 2: Set new code
                Console.WriteLine($">>> FETCH_SUCCESS: {url} | Length: {content.Length}");
            }
            catch (Exception ex)
            {
                SelectedFileContent = $"// ERROR: Target not found at {url}";
                Console.WriteLine($">>> FETCH_ERROR: {ex.Message}");
            }

            NotifyStateChanged();
        }

        public async Task LoadProjects()
        {
            // If we already have projects, don't waste bandwidth fetching again
            if (ProjectFiles.Any()) return;

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            try
            {
                // Adding a timestamp ensures the URL is always unique, forcing a fresh download
                string cacheBuster = DateTime.Now.Ticks.ToString();
                // This pulls from wwwroot/data/projects.json
                var data = await _http.GetFromJsonAsync<List<ProjectFile>>("data/projects.json?v={cacheBuster}", options);

                if (data != null)
                {
                    ProjectFiles = data;
                    Console.WriteLine($">>> SYSTEM: {ProjectFiles.Count} project modules initialized.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($">>> ERROR: Failed to load manifest. {ex.Message}");
            }
            
        }

        public void NotifyStateChanged() => OnChange?.Invoke();
    }
}
