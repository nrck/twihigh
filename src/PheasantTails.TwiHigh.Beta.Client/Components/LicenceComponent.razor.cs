using Microsoft.AspNetCore.Components;
using PheasantTails.TwiHigh.Beta.Client.ViewModels;

namespace PheasantTails.TwiHigh.Beta.Client.Components
{
    public partial class LicenceComponent : UIComponentBase
    {
        [Parameter]
        public Licence Licence { get; set; }
    }
}
