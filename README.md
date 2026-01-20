# MWBlazorPortfolioSite

[Live Demo](https://mikey32905.github.io/MWBlazorPortfolioSite/)

# ⚡ Tactical OS Portfolio (v4.2.0)

A high-fidelity, immersive terminal-based portfolio environment built with **Blazor WebAssembly**. This project simulates a secure tactical operating system, featuring a state-driven authentication layer, a custom synthetic audio engine, and cinematic visual effects.

## 🛠 Technical Stack
* **Frontend Framework:** ASP.NET Core Blazor WebAssembly (.NET 8/9)
* **State Management:** Custom Finite State Machine (FSM) for terminal modes and authentication status.
* **Audio Engine:** Web Audio API integrated via **JavaScript Interop** for real-time synthetic sound generation.
* **UI/UX:** Modern CSS3 with advanced filters (Chromatic Aberration, Gaussian Blur, and Keyframe Animations).
* **Deployment:** GitHub Pages.

## 🚀 Key Features

### 1. State-Driven Authentication & Security
The system utilizes a multi-step login protocol (`AwaitingUsername` -> `AwaitingPassword`) to mimic a real terminal handshake.
* **Security Protocol:** Implements a 3-strike brute-force prevention system.
* **Lockout Mechanism:** A 30-second security freeze featuring an active countdown and a "glitch" visual feedback loop.
* **Administrative Access:** Includes "factory default" credentials and developer cheat codes for rapid testing.

### 2. Synthetic Audio Engine
Instead of relying on static, bulky audio files, this OS uses the **Web Audio API** to generate sound waves mathematically.
* **Dynamic Waveforms:** Utilizes Square waves for harsh system glitches, Triangle waves for a 1.0s "Nuclear Siren," and Sine waves for clean success notifications.
* **Volume Control:** A global gain-handling system with `MUTE`, `UNMUTE`, and `VOLUME [0.0 - 1.0]` command support.

### 3. Cinematic Terminal Experience
* **Auto-Scroll Logic:** Custom JS Interop ensures the command prompt remains anchored during heavy data readouts, providing a true shell experience.
* **Visual Displacement:** Sophisticated "Glitch" and "Melting Text" effects triggered during security breaches.
* **Self-Destruct Sequence:** A scripted cinematic event featuring a screen-shake animation, a recurring siren, and a CRT-style "Signal Loss" dissolve effect that transitions to the boot screen.

## ⌨️ Command Documentation
| Command | Shorthand | Description |
| :--- | :--- | :--- |
| `LOGIN` | `init` | Initiates the secure uplink sequence. |
| `WHOAMI` | - | Displays current operator identity and clearance level. |
| `DOSSIER` | `resume` | Navigates to the professional identity module. |
| `SECRETS` | - | Accesses classified developer notes (Requires Auth). |
| `VOLUME` | - | Adjusts system gain (Usage: `volume 0.5`). |
| `DESTRUCT` | - | Triggers the emergency environment purge protocol. |
| `REBOOT` | `cls` | Cold-starts the system and clears session states. |

## 🧠 Technical Highlights & Learning
Through the development of this project, I deepened my expertise in:
* **Blazor Lifecycle Management:** Mastering `OnAfterRenderAsync` to synchronize the DOM with asynchronous C# logic.
* **C# / JS Interop:** Bridging the gap between .NET managed code and browser-level APIs (Web Audio, Scroll, Focus).
* **Clean Code Refactoring:** Transitioning a complex "God Method" into a modular, routine-based architecture for better maintainability.
* **Immersive UX Design:** Creating a non-traditional interface that remains intuitive for recruiters while maintaining a high-stakes tactical theme.

---

*Built by Mike Williams*
