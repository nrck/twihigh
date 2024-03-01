using Microsoft.AspNetCore.Components.Forms;
using PheasantTails.TwiHigh.Data.Model.TwiHighUsers;
using Reactive.Bindings;

namespace PheasantTails.TwiHigh.BlazorApp.Client.ViewModels;

public interface IProfileEditerViewModel : IViewModelBase
{
    ReactiveCommandSlim AvatarResetCommand { get; }
    ReactivePropertySlim<string> AvatarUrl { get; }
    ReactivePropertySlim<string> Biography { get; }
    ReactivePropertySlim<bool> CanExecuteSaveCommand { get; }
    ReactivePropertySlim<string> DisplayId { get; }
    ReactivePropertySlim<string> DisplayName { get; }
    AsyncReactiveCommand GetUserCommand { get; }
    ReactivePropertySlim<string> HeaderTitle { get; }
    AsyncReactiveCommand<InputFileChangeEventArgs> LoadLocalFileCommand { get; }
    ReactivePropertySlim<PatchTwiHighUserContext.Base64EncodedFileContent?> LocalRawAvatarContent { get; }
    AsyncReactiveCommand SaveCommand { get; }
    ReactivePropertySlim<ResponseTwiHighUserContext?> User { get; }
}
