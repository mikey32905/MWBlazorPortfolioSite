using MWBlazorPortfolioSite.Enums;

namespace MWBlazorPortfolioSite.Services
{
    public record TerminalEntry(string Message, LogType Type, DateTime Timestamp);
    public class TerminalService
    {
        public List<TerminalEntry> Logs { get; } = new();

        // This event notifies the UI to refresh when a new log arrives
        public event Action? OnLogAdded;
        // The "Routine" (Event Delegate)
        public event Action? OnNewLog;

        public void AddLog(string message, LogType type = LogType.System)
        {
            var entry = new TerminalEntry(message, type, DateTime.Now);
            Logs.Add(entry);

            // Keep the terminal performant by limiting history
            if (Logs.Count > 50) Logs.RemoveAt(0);

            // OnLogAdded?.Invoke();
            // This triggers the "Routine" if anyone is listening
            OnNewLog?.Invoke();
        }

        public void LogRequest(string path)
        {
            // This helps you see the absolute URL in the terminal
            AddLog($"FETCH_ATTEMPT: {path}", LogType.System);
        }

        public void ProcessCommand(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return;

            // Log the user's input so it appears in the console
            AddLog($"> {input.ToUpper()}", LogType.System);

            var command = input.ToLower().Trim();

            switch (command)
            {
                case "whoami":
                    AddLog("IDENTITY: [REDACTED] // ARCHITECT_LEVEL_ACCESS", LogType.Success);
                    AddLog("ROLE: FULL_STACK_ENGINEER_V14", LogType.Success);
                    AddLog("STATUS: ACTIVE_PROJECT_MAINTAINER", LogType.Success);
                    break;

                case "clear":
                    Logs.Clear();
                    OnNewLog?.Invoke();
                    break;

                case "help":
                    AddLog("AVAILABLE_CMDS: WHOAMI, CLEAR, HELP, SYSTEM_RESET", LogType.System);
                    break;

                default:
                    AddLog($"ERR: COMMAND '{command}' NOT RECOGNIZED.", LogType.Error);
                    break;
            }
        }
    }
}
