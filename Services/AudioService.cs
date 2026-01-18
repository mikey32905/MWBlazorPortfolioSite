using Microsoft.JSInterop;

namespace MWBlazorPortfolioSite.Services
{
    public class AudioService
    {
        private readonly IJSRuntime _js;
        // Track the mute state
        public bool IsMuted { get; private set; } = false;
        public AudioService(IJSRuntime js) => _js = js;
        public void ToggleMute() => IsMuted = !IsMuted;

        public async Task PlaySystemSound(string soundName)
        {
            // Calls a JS function to play a small beep or chirp
            await _js.InvokeVoidAsync("playSystemAudio", soundName);
        }
    }
}
