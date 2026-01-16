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
window.playSystemAudio = (soundName) => {
    try {
        const audioCtx = new (window.AudioContext || window.webkitAudioContext)();
        const oscillator = audioCtx.createOscillator();
        const gainNode = audioCtx.createGain();

        oscillator.type = 'square';
        oscillator.frequency.setValueAtTime(soundName === 'success' ? 880 : 440, audioCtx.currentTime);

        gainNode.gain.setValueAtTime(0.1, audioCtx.currentTime);
        gainNode.gain.exponentialRampToValueAtTime(0.0001, audioCtx.currentTime + 0.15);

        oscillator.connect(gainNode);
        gainNode.connect(audioCtx.destination);

        oscillator.start();
        oscillator.stop(audioCtx.currentTime + 0.15);
    } catch (e) {
        console.warn("Audio Context blocked by browser policy. User interaction required.");
    }
}; // <--- FIXED: Added missing closing brace