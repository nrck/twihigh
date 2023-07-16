using Microsoft.AspNetCore.Components;
using PheasantTails.TwiHigh.Client.ViewModels;

namespace PheasantTails.TwiHigh.Client.Components
{
    public partial class LicenceComponent : UIComponentBase
    {
        [Parameter]
        public Licence Licence { get; set; }
    }
}
