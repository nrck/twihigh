using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace PheasantTails.TwiHigh.Client.Components
{
    public partial class GapTweetsLoadButtonComponent
    {
        [Parameter]
        public int Index { get; set; }

        [Parameter]
        public DateTimeOffset Since { get; set; }

        [Parameter]
        public DateTimeOffset Until { get; set; }

        [Parameter]
        public OnClickGetGapTweetsAsync? OnClick { get; set; }

        public delegate Task OnClickGetGapTweetsAsync(int index, DateTimeOffset since, DateTimeOffset until);

        private void OnClickGapButton(MouseEventArgs _)
        {
            OnClick?.Invoke(Index, Since, Until);
        }
    }
}
