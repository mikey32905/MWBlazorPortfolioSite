using MWBlazorPortfolioSite.Enums;

namespace MWBlazorPortfolioSite.Services
{
    public record TerminalEntry(string Message, LogType Type, DateTime Timestamp);
    public class TerminalService
    {
        public List<TerminalEntry> Logs { get; } = new();

        // This event notifies the UI to refresh when a new log arrives
        public event Action? OnLogAdded;

        public void AddLog(string message, LogType type = LogType.System)
        {
            var entry = new TerminalEntry(message, type, DateTime.Now);
            Logs.Add(entry);

            // Keep the terminal performant by limiting history
            if (Logs.Count > 50) Logs.RemoveAt(0);

            OnLogAdded?.Invoke();
        }
    }
}
