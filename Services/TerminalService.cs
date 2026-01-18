using MWBlazorPortfolioSite.Enums;


namespace MWBlazorPortfolioSite.Services
{
    public record TerminalEntry(string Message, LogType Type, DateTime Timestamp);
    public class TerminalService
    {
        ProjectStateService _projectState;
        AudioService _audioSvc;
        // Define your "Factory Defaults"
        private const string DEFAULT_USER = "GUEST";
        private const string DEFAULT_PASS = "MIKE";
        private string _tempUsername = ""; // To store username between steps

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

            // Add to history if it's not a duplicate of the last command
            if (!_commandHistory.Any() || _commandHistory.Last() != input)
            {
                _commandHistory.Add(input);
            }

            // Log the user's input so it appears in the console
            AddLog($"> {input.ToUpper()}", LogType.System);

            var command = input.ToLower().Trim();

            if (command == "unlock" || command == "godmode")
            {
                AddLog(">>> EXECUTING_ADMIN_OVERRIDE...", LogType.Warning);
                await Task.Delay(500);
                _projectState.Login(); // Bypass authentication
                _currentStep = LoginStep.None;
                _failedAttempts = 0;
                _isLockedOut = false;
                AddLog("SYSTEM_SECURITY_DEACTIVATED. WELCOME_CREATOR.", LogType.Success);
                return;
            }

            // --- PRE-AUTHENTICATION COMMANDS ---
            switch (command)
            {
                case "forgotusername":
                case "forgotuser":
                    AddLog("RECOVERING_IDENT_HINT...", LogType.Warning);
                    await Task.Delay(500);
                    AddLog("HINT: Standard account for external contractors.", LogType.Info);
                    AddLog("EXPECTED_INPUT: 'GUEST'", LogType.Success);
                    return;

                case "forgotpassword":
                case "forgotpass":
                    AddLog("RECOVERING_AUTH_HINT...", LogType.Warning);
                    await Task.Delay(500);
                    AddLog("HINT: The Architect's first name.", LogType.Info);
                    AddLog("CREDENTIAL_FORMAT: 4_CHAR_STRING", LogType.System);
                    return;
            }

            // --- TWO-STEP LOGIN INTERCEPTOR ---
            if (_currentStep == LoginStep.AwaitingUsername)
            {
                _tempUsername = command.ToUpper();
                AddLog($"> USERNAME: {_tempUsername}", LogType.Info);
                _currentStep = LoginStep.AwaitingPassword;
                AddLog("ENTER_PASSCODE:", LogType.Warning);
                return;
            }

            if (_currentStep == LoginStep.AwaitingPassword)
            {
                // Inside the username/password interceptor logic
                if (_currentStep != LoginStep.None && command == "abort")
                {
                    _currentStep = LoginStep.None;
                    _tempUsername = "";
                    AddLog("LOGIN_SEQUENCE_TERMINATED_BY_OPERATOR.", LogType.Error);
                    return;
                }


                AddLog("> PASSCODE: ********", LogType.Info);

                // Example check: password is "GUEST" or "1234"
                if (_tempUsername == DEFAULT_USER && input.ToUpper() == DEFAULT_PASS)
                {
                    _projectState.Login();
                    _currentStep = LoginStep.None;
                    _failedAttempts = 0;
                    AddLog("ACCESS_GRANTED.", LogType.Success);
                }
                else
                {
                    _failedAttempts++;
                    if (_failedAttempts >= 3)
                    {
                        _ = TriggerLockout(); // Run as fire-and-forget task
                    }
                    else
                    {
                        AddLog($"INVALID_CREDENTIALS. ATTEMPT {_failedAttempts}/3", LogType.Error);
                        _currentStep = LoginStep.AwaitingUsername; // Reset to start
                        AddLog("ENTER_USERNAME:", LogType.Warning);
                    }
                }
                return;
            }



            switch (command)
            {
                case "whoami":
                    AddLog("IDENTITY: MICHAEL_WILLIAMS // ARCHITECT_LEVEL", LogType.Success);
                    break;

                case "login":
                case "init":
                case "start":
                    _currentStep = LoginStep.AwaitingUsername;
                    AddLog("INITIATING_SECURE_LOGIN...", LogType.Warning);
                    AddLog("ENTER_USERNAME:", LogType.Warning);
                    break;

                case "clear":
                case "cls": // Added shorthand
                    Logs.Clear();
                    OnNewLog?.Invoke();
                    break;

                case "help":
                case "?":
                    AddLog("--- TACTICAL_OS_COMMANDS ---", LogType.System);
                    AddLog("HISTORY      - View all session inputs", LogType.Info);
                    AddLog("CLEARHISTORY - Wipe command memory", LogType.Info);
                    AddLog("CLEAR        - Flush terminal screen", LogType.Info);
                    AddLog("REBOOT       - Cold-start system", LogType.Info);
                    AddLog("DOSSIER      - Operator profile", LogType.Info);
                    AddLog("----------------------------", LogType.System);
                    AddLog("HOTKEYS:", LogType.Warning);
                    AddLog("[ESC]        - Close active file", LogType.Warning);
                    AddLog("[UP/DN]      - Cycle commands", LogType.Warning);
                    break;

                case "history":
                    AddLog("--- SESSION_COMMAND_HISTORY ---", LogType.System);
                    if (!_commandHistory.Any())
                    {
                        AddLog("NO_RECORDS_FOUND", LogType.Warning);
                    }
                    else
                    {
                        for (int i = 0; i < _commandHistory.Count; i++)
                        {
                            // Displays a numbered list of previous inputs
                            AddLog($"{i + 1}: {_commandHistory[i]}", LogType.Info);
                        }
                    }
                    break;

                case "clearhistory":
                    _commandHistory.Clear();
                    AddLog("SESSION_HISTORY_PURGED", LogType.Warning);
                    AddLog("UP_ARROW_BUFFER: EMPTY", LogType.Info);
                    break;

                // --- NEW TACTICAL SHORTCUTS ---
                case "hire":
                case "contact":
                case "message":
                    AddLog("ESTABLISHING_SECURE_UPLINK_PROTOCOL...", LogType.Success);
                    OnNavigationRequest?.Invoke("uplink");
                    break;

                case "mute":
                    _audioSvc.ToggleMute(); // Ensure it sets to true
                    AddLog("SYSTEM_AUDIO_TERMINATED", LogType.System);
                    break;

                case "unmute":
                    _audioSvc.ToggleMute(); // Ensure it sets to false
                    AddLog("SYSTEM_AUDIO_INITIALIZED", LogType.System);
                    break;

                case "dossier":
                case "resume":
                    AddLog("REDIRECTING_TO_IDENTITY_MODULE...", LogType.Success);
                    OnNavigationRequest?.Invoke("identity");
                    break;

                 case "reboot":
                 case "system_reset":
                    _projectState.Logout(); // This sets IsAuthenticated to false
                    _currentStep = LoginStep.None;
                    _failedAttempts = 0;
                    OnNavigationRequest?.Invoke("reboot");
                    break;

                case "exit":
                case "close":
                    if (_projectState.SelectedFile != null)
                    {
                        await ExecuteSecureExit();
                    }
                    else
                    {
                        AddLog("ERROR: NO ACTIVE SESSION TO TERMINATE", LogType.Error);
                    }
                    break;

                default:
                    AddLog($"ERR: COMMAND '{command}' NOT RECOGNIZED.", LogType.Error);
                    break;
            }
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
            await _audioSvc.PlaySystemSound("glitch");

            // Live Countdown
            for (int i = 30; i > 0; i--)
            {
                // Only log every 5 or 10 seconds to keep the terminal readable, 
                // or every second if you want the high-pressure feel.
                if (i % 5 == 0 || i <= 5)
                {
                    await _audioSvc.PlaySystemSound("glitch"); 
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
