using Microsoft.AspNetCore.Components;
using PheasantTails.TwiHigh.BlazorApp.Client.Models;
using PheasantTails.TwiHigh.BlazorApp.Client.Views.Bases;
using System.Net.Http.Json;

namespace PheasantTails.TwiHigh.BlazorApp.Client.Views.Pages;

public partial class ShowLicence:TwiHighPageBase
{
    [Inject]
    public HttpClient Client { get; set; } = default!;

    private Licence[]? Licences { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        try
        {
            Licences = await Client.GetFromJsonAsync<Licence[]>($"{Navigation.BaseUri}licenses.json") ?? [];
        }
        catch (Exception)
        {
            Licences = [];
        }
    }
}
