// 1. SCROLL HELPERS
// Consolidated to handle both ID strings and direct Element References
window.scrollToBottom = (elementOrId) => {
    let el = typeof elementOrId === 'string'
        ? document.getElementById(elementOrId)
        : elementOrId;

    if (el) {
        el.scrollTop = el.scrollHeight;
    }
};

window.terminalInterop = {
    scrollToBottom: (element) => {
        if (element) {
            // Use a small timeout to ensure the DOM has finished 
            // the 'Glitch' render and added the new log lines
            setTimeout(() => {
                element.scrollTo({
                    top: element.scrollHeight,
                    behavior: 'auto' // 'smooth' can sometimes fail if many logs arrive at once
                });
            }, 10);
        }
    }
};

window.focusElement = (elementId) => {
    const element = document.getElementById(elementId);
    if (element) element.focus();
};

// 2. UI POLISH
window.killDoubleScrollbars = () => {
    const targets = ['.main-stage', '.data-pane', '.transparent-editor'];
    targets.forEach(selector => {
        const el = document.querySelector(selector);
        if (el) { el.style.overflow = 'hidden'; }
    });
};

// Initialize observer for scrollbar cleanup
window.onload = () => {
    const observer = new MutationObserver(() => window.killDoubleScrollbars());
    observer.observe(document.body, { childList: true, subtree: true });
};

// 3. STORAGE UTILS
window.localStorageFunctions = {
    setItem: function (key, value) {
        localStorage.setItem(key, value);
    },
    getItem: function (key) {
        return localStorage.getItem(key);
    }
};

// 4. AUDIO ENGINE
window.playSystemAudio = async (soundName, volume = 0.1) => {
    try {
        const audioCtx = new (window.AudioContext || window.webkitAudioContext)();
        if (audioCtx.state === 'suspended') await audioCtx.resume();

        const oscillator = audioCtx.createOscillator();
        const gainNode = audioCtx.createGain();

        // Use the passed volume parameter
        gainNode.gain.setValueAtTime(volume, audioCtx.currentTime);

        if (soundName === "siren") {
            oscillator.type = 'triangle';
            // Slide from low to high frequency (The "Wail")
            oscillator.frequency.setValueAtTime(300, audioCtx.currentTime);
            oscillator.frequency.exponentialRampToValueAtTime(800, audioCtx.currentTime + 0.5);
            oscillator.frequency.exponentialRampToValueAtTime(300, audioCtx.currentTime + 1.0);

            gainNode.gain.exponentialRampToValueAtTime(0.01, audioCtx.currentTime + 1.0);

            oscillator.connect(gainNode);
            gainNode.connect(audioCtx.destination);

            oscillator.start();
            oscillator.stop(audioCtx.currentTime + 1.0);
            return; // Exit here so the default start/stop at bottom doesn't interfere
        }
        else if (soundName === "glitch") {
            oscillator.type = 'square';
            oscillator.frequency.setValueAtTime(120, audioCtx.currentTime);
            oscillator.frequency.exponentialRampToValueAtTime(40, audioCtx.currentTime + 0.2);
            gainNode.gain.exponentialRampToValueAtTime(0.01, audioCtx.currentTime + 0.2);
        }
        else {
            oscillator.type = 'sine';
            oscillator.frequency.setValueAtTime(soundName === 'success' ? 880 : 440, audioCtx.currentTime);
            gainNode.gain.exponentialRampToValueAtTime(0.0001, audioCtx.currentTime + 0.15);
        }

        // Standard connection and start/stop for glitch and beeps
        oscillator.connect(gainNode);
        gainNode.connect(audioCtx.destination);
        oscillator.start();
        oscillator.stop(audioCtx.currentTime + 0.2);

    } catch (e) {
        console.warn("Audio Context blocked:", e);
    }
};