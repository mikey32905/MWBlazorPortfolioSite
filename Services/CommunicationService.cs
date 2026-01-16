using Microsoft.JSInterop;
using MWBlazorPortfolioSite.Models;

namespace MWBlazorPortfolioSite.Services
{
    public class CommunicationService
    {
        private readonly IJSRuntime _js;
        private readonly TerminalService _terminal;
        private readonly AudioService _audio;

        // Add this property to track the mute state
        public bool IsMuted { get; set;} = false; 
        public int TransmissionCount { get; private set; } = 0;
        public event Action? OnTransmissionSuccess;

        public CommunicationService(IJSRuntime js, TerminalService terminal, AudioService audio)
        {
            _js = js;
            _terminal = terminal;
            _audio = audio;
        }

        public async Task<bool> SendUplinkMessage(ContactForm contactModel)//string name, string email, string message
        {
            var templateParams = new
            {
                from_name = contactModel.Name,
                reply_to = contactModel.Email,
                message = contactModel.Message
            };

            // Use the same JS function name we defined in index.html
            bool success = await _js.InvokeAsync<bool>("sendEmail", "service_lcjfrme",
                    "template_wxstuk8", templateParams);

            if (success)
            {
                if (!IsMuted)
                {
                    await _audio.PlaySystemSound("success");
                }

                TransmissionCount++;
                OnTransmissionSuccess?.Invoke();
            }


            return success;
        }
    }
}
