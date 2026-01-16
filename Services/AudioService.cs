using Microsoft.JSInterop;

namespace MWBlazorPortfolioSite.Services
{
    public class AudioService
    {
        private readonly IJSRuntime _js;

        public AudioService(IJSRuntime js) => _js = js;

        public async Task PlaySystemSound(string soundName)
        {
            // Calls a JS function to play a small beep or chirp
            await _js.InvokeVoidAsync("playSystemAudio", soundName);
        }
    }
}
