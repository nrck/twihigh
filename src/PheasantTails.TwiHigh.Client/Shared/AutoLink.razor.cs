using Microsoft.AspNetCore.Components;
using System.Text.RegularExpressions;

namespace PheasantTails.TwiHigh.Client.Shared
{
    public partial class AutoLink
    {
        [Parameter]
        public string Text { get; set; }

        [Parameter]
        public bool ReplaceDisplayId { get; set; }

        private RenderFragment RenderFragment { get; set; }

        private string Content { get; set; }

        protected override void OnInitialized()
        {
            string text = Text;
            if (ReplaceDisplayId)
            {
                Content = Regex.Replace(text, "@([a-zA-Z0-9._-]+)", $"<a href=\"profile/$1\" target=\"_blank\">@$1</a>");
            }
            RenderFragment = builder =>
            {
                builder.OpenElement(0, "div");
                builder.AddMarkupContent(1, text);
                builder.CloseElement();
            };
            base.OnInitialized();
        }
    }
}
