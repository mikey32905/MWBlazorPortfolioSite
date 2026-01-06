using MWBlazorPortfolioSite.Models;

namespace MWBlazorPortfolioSite.Services
{
    public class ProjectStateService
    {
        public ProjectFile? SelectedFile { get; private set; }
        public event Action? OnChange;

        public void SelectFile(ProjectFile file)
        {
            SelectedFile = file;
            NotifyStateChanged();
        }

        private void NotifyStateChanged() => OnChange?.Invoke();
    }
}
