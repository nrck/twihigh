using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using PheasantTails.TwiHigh.Beta.Client.ViewModels;

namespace PheasantTails.TwiHigh.Beta.Client.Components
{
    public partial class GapTweetsLoadButtonComponent
    {
        [Parameter]
        public TweetViewModel Tweet { get; set; }

        [Parameter]
        public EventCallback<TweetViewModel> OnClick { get; set; }

        private Task OnClickGapButton(MouseEventArgs _) => OnClick.InvokeAsync(Tweet);
    }
}
