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

        //public void SelectFile(ProjectFile file)
        //{
        //    SelectedFile = file;

        //    if (file != null)
        //    {
        //        CurrentAccentColor = file.AccentColor;
        //        // ... any other logic that requires a file ...
        //        Console.WriteLine($">>> STATE_SERVICE: Selected {file.FileName}");
        //    }
        //    else
        //    {
        //        // Reset to default "Welcome" state values
        //        CurrentAccentColor = "#00FF41";
        //        Console.WriteLine(">>> STATE_SERVICE: Session Closed / File Cleared");
        //    }

        //    // Fire EVERY messenger to ensure all components hear the update
        //    NotifyStateChanged(); // Fires OnChange
        //                          // OnSelectedFileChanged?.Invoke();

        //    Console.WriteLine(">>> STATE_SERVICE: Session Closed / File Cleared");
        //}

        public async Task SelectFile(ProjectFile? file)
        {
            SelectedFile = file;
            SelectedFileContent = null; // Clear old content immediately

            //if (file != null && !string.IsNullOrEmpty(file.SourceUrl))
            //{
            //    Console.WriteLine($">>> SYSTEM: Attempting to fetch source from {file.SourceUrl}");
            //    try
            //    {
            //        SelectedFileContent = await _http.GetStringAsync(file.SourceUrl);
            //        Console.WriteLine(">>> SYSTEM: Source code successfully cached.");
            //    }
            //    catch (Exception ex)
            //    {
            //        SelectedFileContent = $"// ERROR: Could not load {file.SourceUrl}. {ex.Message}";
            //        Console.WriteLine($">>> ERROR: {ex.Message}");
            //    }
            //}
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
            //else
            //{
            //    CurrentAccentColor = "#00FF41";
            //}

            NotifyStateChanged();
        }

        // Call this after fetching the JSON in your initialization logic
        //public void LoadProjects(List<ProjectFile> files)
        //{
        //    ProjectFiles = files;
        //}

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
                // This pulls from wwwroot/data/projects.json
                var data = await _http.GetFromJsonAsync<List<ProjectFile>>("data/projects.json", options);

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
            //var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            //// Fetch and deserialize the manifest
            //var projects = await _http.GetFromJsonAsync<List<ProjectFile>>("data/projects.json", options);

            //// Store them in your service...
            //ProjectFiles = projects;
        }

        public void NotifyStateChanged() => OnChange?.Invoke();
    }
}
