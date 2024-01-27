using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using PheasantTails.TwiHigh.BlazorApp.Client.Models;
using PheasantTails.TwiHigh.BlazorApp.Client.Views.Bases;

namespace PheasantTails.TwiHigh.BlazorApp.Client.Views.Components
{
    public partial class THGapTweetsFetchButton : TwiHighComponentBase
    {
        [Parameter]
        public DisplayTweet Tweet { get; set; }

        [Parameter]
        public EventCallback<DisplayTweet> OnClick { get; set; }

        private Task OnClickGapButton(MouseEventArgs _) => OnClick.InvokeAsync(Tweet);
    }
}
