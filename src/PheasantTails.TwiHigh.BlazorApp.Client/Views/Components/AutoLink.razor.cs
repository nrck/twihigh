using Microsoft.AspNetCore.Components;
using System.Text.RegularExpressions;

namespace PheasantTails.TwiHigh.BlazorApp.Client.Views.Components
{
    public partial class AutoLink
    {
        [Parameter]
        public string Text { get; set; } = string.Empty;

        [Parameter]
        public bool ReplaceDisplayId { get; set; } = true;

        [Parameter]
        public bool ReplaceUrl { get; set; } = true;

        private string ContentString { get; set; } = string.Empty;

        private string[] ContentArray { get; set; } = Array.Empty<string>();

        protected override void OnParametersSet()
        {
            ContentString = Text;
            if (ReplaceDisplayId)
            {
                //Content = Regex.Replace(Content, "@([a-zA-Z0-9._-]+)", "<a href=\"/profile/$1\">@$1</a>");
                ContentString = Regex.Replace(ContentString, "@([a-zA-Z0-9._-]+)", $"{Environment.NewLine}$1{Environment.NewLine}");
            }
            if (ReplaceUrl)
            {
                ContentString = Regex.Replace(ContentString, "(https?://[\\w/:%#\\$&\\?\\(\\)~\\.=\\+\\-]+)", " <a href=\"$1\" target=\"_blank\" onclick=\"event.stopPropagation()\">$1</a> ");
            }

            ContentArray = ContentString.Split(Environment.NewLine);

            StateHasChanged();
            base.OnParametersSet();
        }

        private RenderFragment GetRenderFragment() => builder =>
        {
            var sequence = 0;
            for (int index = 0; index < ContentArray.Length; index++)
            {
                if (index % 2 == 0)
                {
                    builder.AddMarkupContent(sequence, ContentArray[index]);
                    sequence++;
                }
                else
                {
                    builder.OpenComponent<THUserIdLink>(sequence);
                    sequence++;
                    builder.AddAttribute(sequence, "UserDisplayId", ContentArray[index]);
                    sequence++;
                    builder.CloseComponent();
                }
            }
        };
    }
}
