using Microsoft.AspNetCore.Components;
using PheasantTails.TwiHigh.DataStore.Entity;

namespace PheasantTails.TwiHigh.Client.Components
{
    public partial class TweetComponent
    {
        [Parameter]
        public Tweet? Tweet { get; set; }
    }
}
