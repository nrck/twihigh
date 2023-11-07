using Microsoft.AspNetCore.Components;

namespace PheasantTails.TwiHigh.Beta.Client.Components
{
    public partial class PageTitleComponent
    {
        [Parameter]
        public string Title { get; set; } = string.Empty;
    }
}
