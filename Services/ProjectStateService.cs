using MWBlazorPortfolioSite.Models;

namespace MWBlazorPortfolioSite.Services
{
    public class ProjectStateService
    {
        public ProjectFile? SelectedFile { get; private set; }
        // The full list of all projects/files from your JSON
        public List<ProjectFile> ProjectFiles { get; set; } = new();

        public event Action? OnChange;
        // The "Messenger": Components subscribe to this to know when to react
        public event Action? OnSelectedFileChanged;

        public void SelectFile(ProjectFile file)
        {
            SelectedFile = file;
            NotifyStateChanged();

            // Notify all listeners (like the MainLayout menu) that a change happened
            OnSelectedFileChanged?.Invoke();
        }

        // Call this after fetching the JSON in your initialization logic
        public void LoadProjects(List<ProjectFile> files)
        {
            ProjectFiles = files;
        }

        private void NotifyStateChanged() => OnChange?.Invoke();
    }
}
