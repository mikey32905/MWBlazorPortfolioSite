using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MWBlazorPortfolioSite.Enums;


namespace MWBlazorPortfolioSite.Services
{
    public record TerminalEntry(string Message, LogType Type, DateTime Timestamp);
    public class TerminalService
    {
        ProjectStateService _projectState;
        AudioService _audioSvc;
        private readonly IJSRuntime _js;
        private readonly NavigationManager _navManager;
        // Define your "Factory Defaults"
        private const string DEFAULT_USER = "GUEST";
        private const string DEFAULT_PASS = "MIKE";
        private string _tempUsername = ""; // To store username between steps
        public bool IsDissolving { get; private set; } = false;
        public List<TerminalEntry> Logs { get; } = new();

        private List<string> _commandHistory = new();
        // Allows the UI to read the history for arrow navigation
        public IReadOnlyList<string> CommandHistory => _commandHistory;

        #region Event Members

        // This event notifies the UI to refresh when a new log arrives
        public event Action? OnLogAdded;
        // The "Routine" (Event Delegate)
        public event Action? OnNewLog;
        // New event to tell the UI to change pages
        public event Action<string>? OnNavigationRequest;
        // ADD THIS: A dedicated event just for the reboot sequence
        public static event Action? OnSystemResetRequested;
        // Define the event so 'OnChange?.Invoke()' has a target
        public event Action? OnChange;

        #endregion Event Members

        private LoginStep _currentStep = LoginStep.None;
        private int _failedAttempts = 0;
        private bool _isLockedOut = false;

        public bool IsLockedOut => _isLockedOut; // Expose to the Razor component
        public bool IsAwaitingPassword => _currentStep == LoginStep.AwaitingPassword;

        public TerminalService(ProjectStateService projectState, AudioService audioSvc)
        {
            _projectState = projectState;
            _audioSvc = audioSvc;
            // Subscribe so the terminal knows when files are opened OR closed globally
            _projectState.OnChange += HandleGlobalStateChange;
        }

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

        public void AddToTerminal(string message)
        {
            // Create the object the list is looking for
            var entry = new TerminalEntry(message, LogType.System, DateTime.Now);
            
            Logs.Add(entry);

            // Keep only the last 50 logs to prevent memory lag
            if (Logs.Count > 50) Logs.RemoveAt(0);

            NotifyStateChanged();
        }

        public void Clear()
        {
            Logs.Clear();
            NotifyStateChanged();
        }

        public void LogRequest(string path)
        {
            // This helps you see the absolute URL in the terminal
            AddLog($"FETCH_ATTEMPT: {path}", LogType.System);
        }

        private void HandleGlobalStateChange()
        {
            if (_projectState.SelectedFile == null)
            {
                AddLog(">>> SYSTEM_EVENT: GLOBAL_SESSION_TERMINATED", LogType.System);
            }
        }

        public async Task ProcessCommand(string input)
        {
            if (_isLockedOut)
            {
                AddLog("ERR: SYSTEM_FROZEN. SECURITY_COOLDOWN_ACTIVE.", LogType.Error);
                return;
            }
            if (string.IsNullOrWhiteSpace(input)) return;

            UpdateHistory(input);
            var command = input.ToLower().Trim();

            // This one block handles ALL special states (Login, Password, and Destruct)
            if (_currentStep != LoginStep.None)
            {
                await HandleLoginFlow(command, input);
                return;
            }

            if (await HandleGlobalCommands(command)) return;

            await HandleStandardCommands(command, input);
        }

        private void UpdateHistory(string input)
        {
            if (!_commandHistory.Any() || _commandHistory.Last() != input)
            {
                _commandHistory.Add(input);
            }
            AddLog($"> {input.ToUpper()}", LogType.System);
        }

        private async Task<bool> HandleGlobalCommands(string command)
        {
            switch (command)
            {
                case "unlock":
                case "godmode":
                    await ExecuteAdminOverride();
                    return true;
                case "forgotuser":
                case "forgotusername":
                    AddLog("HINT: Standard account for external contractors. 'GUEST'", LogType.Info);
                    return true;
                case "forgotpass":
                case "forgotpassword":
                    AddLog("HINT: The Architect's first name. (4 chars)", LogType.Info);
                    return true;
                default:
                    return false;
            }
        }

        private async Task HandleLoginFlow(string command, string originalInput)
        {
            // Handle Self-Destruct State
            if (_currentStep == LoginStep.AwaitingDestructConfirmation)
            {
                await HandleDestructConfirmation(command);
                return;
            }

            if (command == "abort")
            {
                _currentStep = LoginStep.None;
                _tempUsername = "";
                AddLog("LOGIN_SEQUENCE_TERMINATED.", LogType.Error);
                return;
            }

            if (_currentStep == LoginStep.AwaitingUsername)
            {
                _tempUsername = command.ToUpper();
                AddLog($"> USERNAME: {_tempUsername}", LogType.Info);
                _currentStep = LoginStep.AwaitingPassword;
                AddLog("ENTER_PASSCODE:", LogType.Warning);
            }
            else if (_currentStep == LoginStep.AwaitingPassword)
            {
                AddLog("> PASSCODE: ********", LogType.Info);
                if (_tempUsername == DEFAULT_USER && originalInput.ToUpper() == DEFAULT_PASS)
                {
                    _projectState.Login();
                    _currentStep = LoginStep.None;
                    _failedAttempts = 0;
                    AddLog("ACCESS_GRANTED.", LogType.Success);
                }
                else
                {
                    _failedAttempts++;
                    if (_failedAttempts >= 3) _ = TriggerLockout();
                    else
                    {
                        AddLog($"INVALID_CREDENTIALS. ATTEMPT {_failedAttempts}/3", LogType.Error);
                        _currentStep = LoginStep.AwaitingUsername;
                        AddLog("RE-ENTER_USERNAME:", LogType.Warning);
                    }
                }
            }
        }

        private async Task HandleStandardCommands(string command, string originalInput)
        {
            // Block unauthorized standard commands
            if (!_projectState.IsAuthenticated && !new[] { "login", "init", "start", "reboot" }.Contains(command))
            {
                AddLog("ERR: SYSTEM_LOCKED. INITIALIZE 'LOGIN'.", LogType.Error);
                return;
            }

            switch (command)
            {
                case "help":
                    DisplayHelp();
                    break;
                case "login":
                case "init":
                case "start":
                    _currentStep = LoginStep.AwaitingUsername;
                    AddLog("ENTER_USERNAME:", LogType.Warning);
                    break;
                case "reboot":
                    _projectState.Logout();
                    _currentStep = LoginStep.None;
                    OnNavigationRequest?.Invoke("reboot");
                    break;
                case "dossier":
                case "personnel_dossier":
                case "resume":
                    AddLog("ACCESSING_PERSONNEL_FILES...", LogType.Success);
                    // Point this to your existing Identity route
                    OnNavigationRequest?.Invoke("identity");
                    break;
                case "exit":
                case "close":
                    await ExecuteSecureExit();
                    break;
                case "clear":
                case "cls":
                    Logs.Clear();
                    OnNewLog?.Invoke();
                    break;
                case "volume":
                    var parts = originalInput.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length > 1 && double.TryParse(parts[1], out var vol))
                    {
                        // Clamp between 0.0 and 1.0
                        _audioSvc.Volume = Math.Clamp(vol, 0.0, 1.0);
                        AddLog($"SYSTEM_GAIN_SET: {_audioSvc.Volume * 100}%", LogType.System);
                        await _audioSvc.PlaySystemSound("success", _audioSvc.Volume);
                    }
                    else
                    {
                        AddLog("USAGE: VOLUME [0.0 - 1.0]", LogType.Error);
                    }
                    break;
                case "mute":
                case "unmute":
                    _audioSvc.ToggleMute();
                    AddLog(_audioSvc.IsMuted ? "AUDIO_OFF" : "AUDIO_ON", LogType.System);
                    break;
                case "secrets":
                case "dev_notes":
                    if (_projectState.IsAuthenticated)
                    {
                        AddLog("--- CLASSIFIED_SYSTEM_NOTES ---", LogType.Warning);
                        AddLog("DESTRUCT - Authorized Emergency Purge Protocol.", LogType.Info);
                        AddLog("GODMODE  - Admin Login Bypass (For Testing Only).", LogType.Info);
                        AddLog("CREDENTIALS_RECOVERY: 'FORGOTUSER' / 'FORGOTPASS'", LogType.Info);
                    }
                    else
                    {
                        AddLog("ERR: ACCESS_DENIED. AUTHENTICATION_REQUIRED.", LogType.Error);
                    }
                    break;
                case "destruct":
                case "self_destruct":
                    _currentStep = LoginStep.AwaitingDestructConfirmation;
                    AddLog("!!! WARNING: THIS WILL RESET THE ENTIRE ENVIRONMENT !!!", LogType.Error);
                    AddLog("ARE YOU ABSOLUTELY SURE? (Y/N)", LogType.Warning);
                    break;
                default:
                    AddLog($"ERR: COMMAND '{command}' NOT RECOGNIZED.", LogType.Error);
                    break;
            }
        }

        private async Task HandleDestructConfirmation(string command)
        {
            if (command == "y" || command == "yes")
            {
                _currentStep = LoginStep.None;
                await ExecuteSelfDestruct();
            }
            else
            {
                _currentStep = LoginStep.None;
                AddLog("SELF_DESTRUCT_SEQUENCE_ABORTED.", LogType.Success);
                AddLog("SYSTEM_INTEGRITY_STABLE.", LogType.Info);
            }
        }

        private void DisplayHelp()
        {
            AddLog("--- TACTICAL_OS_COMMANDS ---", LogType.System);
            AddLog("HISTORY      - View session input log", LogType.Info);
            AddLog("CLEARHISTORY - Wipe command memory", LogType.Info);
            AddLog("CLEAR        - Flush terminal buffer", LogType.Info);
            AddLog("REBOOT       - Cold-start system cycle", LogType.Info);
            AddLog("DOSSIER      - Access operator profile", LogType.Info);
            AddLog("SECRETS      - Access Dev Notes", LogType.Info);
            AddLog("MUTE/UNMUTE  - Toggle system acoustics", LogType.Info);
            AddLog("EXIT/CLOSE   - Terminate active session", LogType.Info);
            AddLog("----------------------------", LogType.System);
            AddLog("HOTKEYS:", LogType.Warning);
            AddLog("[ESC]        - Force close file", LogType.Warning);
            AddLog("[UP/DN]      - Cycle command buffer", LogType.Warning);
        }

        private async Task ExecuteAdminOverride()
        {
            AddLog(">>> EXECUTING_ADMIN_OVERRIDE...", LogType.Warning);

            // Play the success beep to signal the bypass worked
            await _audioSvc.PlaySystemSound("success", 0.3);

            await Task.Delay(800);

            _projectState.Login(); // Bypass auth state
            _currentStep = LoginStep.None;
            _failedAttempts = 0;
            _isLockedOut = false;

            AddLog("SYSTEM_SECURITY_DEACTIVATED.", LogType.Success);
            AddLog("WELCOME_CREATOR. ACCESS_RESTRICTIONS_LIFTED.", LogType.Success);
        }

        private async Task ExecuteSelfDestruct()
        {
            AddLog("!!! EMERGENCY_OVERRIDE_INITIATED !!!", LogType.Error);
            _isLockedOut = true;

            for (int i = 5; i > 0; i--)
            {
                AddLog($"CRITICAL_FAILURE_IN: {i}...", LogType.Error);

                // Trigger the siren sound
                await _audioSvc.PlaySystemSound("siren", 0.4);

                // On the final second, trigger the dissolve
                if (i == 1) { IsDissolving = true; NotifyStateChanged(); }

                await Task.Delay(1000);
            }

            AddLog("TERMINATING_CORE_PROCESSES...", LogType.Warning);
            await Task.Delay(1000);

            // Reboot
            _projectState.Logout();

            // 2. Instead of location.assign, use the NavigationManager to force a reload
            // 'true' forces a browser refresh, bypassing the internal Blazor router
            OnNavigationRequest?.Invoke( "reboot");//"/reboot"_navManager.BaseUri +



            // Reset state for next boot
            IsDissolving = false;
            _isLockedOut = false;
        }

        // New helper method for the "theatrical" exit
        private async Task ExecuteSecureExit()
        {
            AddLog("INITIATING_SESSION_TEARDOWN...", LogType.Warning);
            await Task.Delay(400);
            AddLog("SYNCING_LOCAL_BUFFER: OK", LogType.Info);
            await Task.Delay(300);
            AddLog("DE-SEGMENTING_MEMORY_BLOCKS...", LogType.Info);
            await Task.Delay(500);

            // Trigger the actual global close
            _projectState.CloseFile();

            // FORCE NAVIGATION to Home/Standby
            // This solves the issue of being "stuck" on the Uplink or WPF page
            OnNavigationRequest?.Invoke("");

            // Add a final log and FORCE the terminal to refresh its UI
            AddLog(">>> SESSION_TERMINATED. STANDBY_MODE_ACTIVE.", LogType.System);

            // CRITICAL: Ensure the UI knows something changed
            NotifyStateChanged();
            OnNewLog?.Invoke();
        }

        private async Task TriggerLockout()
        {
            _isLockedOut = true;
            _currentStep = LoginStep.None;

            AddLog("!!! SECURITY_BREACH_DETECTED !!!", LogType.Error);
            AddLog("BRUTE_FORCE_PREVENTION_ACTIVE.", LogType.Error);

            // Initial alert sound
            await _audioSvc.PlaySystemSound("glitch", 0.3);

            // Live Countdown
            for (int i = 30; i > 0; i--)
            {
                // Only log every 5 or 10 seconds to keep the terminal readable, 
                // or every second if you want the high-pressure feel.
                if (i % 5 == 0 || i <= 5)
                {
                    await _audioSvc.PlaySystemSound("glitch", 0.3); 
                    AddLog($"SYSTEM_LOCK: {i}s REMAINING...", LogType.Warning);
                    OnNewLog?.Invoke();
                }

                // This notifies the UI to show the new "seconds" log immediately
               // OnNewLog?.Invoke();

                await Task.Delay(1000); // Wait 1 second
            }

            _isLockedOut = false;
            _failedAttempts = 0;
            AddLog(">>> SECURITY_RESTORED. RETRY_LOGIN.", LogType.Success);
            OnNewLog?.Invoke();
        }

        private void NotifyStateChanged() => OnChange?.Invoke();
    }
}
