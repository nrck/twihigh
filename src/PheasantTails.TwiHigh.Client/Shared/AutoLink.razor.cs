using Microsoft.AspNetCore.Components;
using System.Text.RegularExpressions;

namespace PheasantTails.TwiHigh.Client.Shared
{
    public partial class AutoLink
    {
        [Parameter]
        public string Text { get; set; } = string.Empty;

        [Parameter]
        public bool ReplaceDisplayId { get; set; } = true;

        [Parameter]
        public bool ReplaceUrl { get; set; } = true;

        private string Content { get; set; } = string.Empty;

        protected override void OnParametersSet()
        {
            Content = Text;
            if (ReplaceDisplayId)
            {
                Content = Regex.Replace(Content, "@([a-zA-Z0-9._-]+)", "<a href=\"profile/$1\">@$1</a>");
            }
            if (ReplaceUrl)
            {
                Content = Regex.Replace(Content, "(https?://[\\w/:%#\\$&\\?\\(\\)~\\.=\\+\\-]+)", " <a href=\"$1\" target=\"_blank\">$1</a> ");
            }

            StateHasChanged();
            base.OnParametersSet();
        }
    }
}
