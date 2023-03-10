using Microsoft.AspNetCore.Components.Web;
using PheasantTails.TwiHigh.Data.Model.TwiHighUsers;

namespace PheasantTails.TwiHigh.Client.Pages
{
    public partial class Signup
    {
        private AddTwiHighUserContext Context { get; set; } = new AddTwiHighUserContext();
        private bool IsWorking { get; set; } = false;
        private string ErrorMessage { get; set; } = string.Empty;

        private async Task OnClickSignupButtonAsync(MouseEventArgs _)
        {
            ErrorMessage = string.Empty;
            IsWorking = true;
            
            await Task.Delay(500);
            IsWorking = false;
        }
    }
}
