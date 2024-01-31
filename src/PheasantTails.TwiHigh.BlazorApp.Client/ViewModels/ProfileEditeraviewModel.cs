using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using PheasantTails.TwiHigh.BlazorApp.Client.Services;
using PheasantTails.TwiHigh.Data.Model.TwiHighUsers;
using Reactive.Bindings;

namespace PheasantTails.TwiHigh.BlazorApp.Client.ViewModels;

public class ProfileEditeraviewModel : ViewModelBase
{
    private PatchTwiHighUserContext _patchContext;


    public ReactivePropertySlim<ResponseTwiHighUserContext> User { get; private set; } = default!;
    public ReactivePropertySlim<string> HeaderTitle { get; private set; } = default!;
    public ReactivePropertySlim<string> AvatarUrl { get; private set; } = default!;
    public ReactivePropertySlim<string> DisplayId { get; private set; } = default!;
    public ReactivePropertySlim<string> DisplayName { get; private set; } = default!;
    public ReactivePropertySlim<string> Biography { get; private set; } = default!;
    public ReactivePropertySlim<byte[]> LocalRawAvatarData { get; private set; } = default!;
    public ReactivePropertySlim<string> LocalRowAvatarContentType { get; private set; } = default!;

    public AsyncReactiveCommand<InputFileChangeEventArgs> LoadLocalFileCommand { get; private set; } = default!;
    public AsyncReactiveCommand SaveCommand { get; private set; } = default!;
    public ReactiveCommandSlim AvatarResetCommand { get; private set; } = default!;

    public ProfileEditeraviewModel(NavigationManager navigationManager, IMessageService messageService)
        : base(navigationManager, messageService)
    {
        _patchContext = new();
    }

    protected override void Initialize()
    {
    }

    protected override void Subscribe()
    {
    }
}
