using MWBlazorPortfolioSite.Models;

namespace MWBlazorPortfolioSite.Services
{
    public class ProjectStateService
    {
        public ProjectFile? SelectedFile { get; private set; }
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

        private void NotifyStateChanged() => OnChange?.Invoke();
    }
}
